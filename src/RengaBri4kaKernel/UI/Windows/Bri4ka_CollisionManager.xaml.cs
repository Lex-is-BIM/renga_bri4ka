using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Functions;
using RengaBri4kaKernel.AuxFunctions;
using Renga;

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_ClashDetective.xaml
    /// </summary>
    public partial class Bri4ka_ClashDetective : Window, IWindowWithConfig<ClashDetectiveConfig>
    {
        public Bri4ka_ClashDetective()
        {
            InitializeComponent();
            pRengaTypes = RengaUtils.GetRengaObjectTypes();
            setObjectTypes(this.ListBox_Group1);
            setObjectTypes(this.ListBox_Group2);

            ClashDetectiveConfig? config = (ClashDetectiveConfig?)ConfigIO.LoadFrom<ClashDetectiveConfig>(ConfigIO.GetDefaultPath<ClashDetectiveConfig>());
            if (config == null) config = new ClashDetectiveConfig();

            SetConfigToUi(config);
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private Guid[] getSelectedObjectTypes(ListBox box)
        {
            List<Guid> selectedObjects = new List<Guid>();
            var selected = box.SelectedItems;
            foreach (ListViewItem node in box.Items)
            {
                if (node.IsSelected) selectedObjects.Add((Guid)node.Tag);
            }

            return selectedObjects.ToArray();
        }

        public void setObjectTypes(ListBox box)
        {
            box.SelectionMode = SelectionMode.Multiple;
            box.Items.Clear();
            foreach (var type in pRengaTypes)
            {
                ListViewItem item = new ListViewItem();
                item.Tag = type.Id;
                item.Content = type.Name;
                box.Items.Add(item);
            }
        }

        private void UpdatePropertiesForGroup1()
        {
            if (this.ListBox_Group1.SelectedItems.Count == 0) return;
            var selectedTypes = getSelectedObjectTypes(this.ListBox_Group1);

            // Для заданных типов объектов нужно показать свойства
            var relevantProps = RengaPropertiesUtils.GetPropertiesInfoByTypes(selectedTypes);
            if (relevantProps == null) return;
            if (!relevantProps.Any()) return;

            this.ListBox_ParametersGroup1.Items.Clear();

            foreach (PropertyInfo prop in relevantProps)
            {
                ListViewItem item = new ListViewItem();
                item.Tag = prop.Id;
                item.Content = prop.Name;
                item.IsSelected = false;
                this.ListBox_ParametersGroup1.Items.Add(item);
            }
        }

        #region Handlers
        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            ClashDetectiveConfig config = this.GetConfigFromUI();
            if (!config.Group1.Any())
            {
                throw new Exception("RengaBri4ka. Не заданы объекты первой группы!");
            }
            if (!config.Group2.Any())
            {
                throw new Exception("RengaBri4ka. Не заданы объекты второй группы!");
            }

            ConfigIO.SaveTo<ClashDetectiveConfig>(ConfigIO.GetDefaultPath<ClashDetectiveConfig>(), config);

            RengaCollisionManager func = new RengaCollisionManager();
            func.Start(config);

            this.DialogResult = true;
            this.Close();
        }
        private void ListBox_Group1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePropertiesForGroup1();
            
        }

        private void Button_ReverseChoosing12_Click(object sender, RoutedEventArgs e)
        {
            var gr1 = getSelectedObjectTypes(this.ListBox_Group1);
            var gr2 = getSelectedObjectTypes(this.ListBox_Group2);

            setObjectTypesSelected(this.ListBox_Group1, gr2);
            setObjectTypesSelected(this.ListBox_Group2, gr1);
        }
        #endregion

        #region Config

        private void setObjectTypesSelected(ListBox box, Guid[]? ids)
        {
            if (ids == null) return;
            foreach (ListViewItem item in box.Items)
            {
                Guid itemGuid = (Guid)item.Tag;
                if (ids.Contains(itemGuid)) item.IsSelected = true;
                else item.IsSelected = false;
            }
        }

        public ClashDetectiveConfig GetConfigFromUI()
        {
            ClashDetectiveConfig config = new ClashDetectiveConfig();

            config.Name = this.TextBox_ClashName.Text;

            config.Group1 = getSelectedObjectTypes(this.ListBox_Group1);
            config.Group2 = getSelectedObjectTypes(this.ListBox_Group2);

            //config.ClashSettings.Separate = this.CheckBox_GeomMode_Separate.IsChecked ?? false;
            config.ClashSettings.Touching = this.CheckBox_GeomMode_Touching.IsChecked ?? false;
            config.ClashSettings.Intersecting = this.CheckBox_GeomMode_Intersecting.IsChecked ?? false;
            config.ClashSettings.Contains = this.CheckBox_GeomMode_Contains.IsChecked ?? false;
            config.ClashSettings.ContainedBy = this.CheckBox_GeomMode_ContainedBy.IsChecked ?? false;
            config.ClashSettings.Equal = this.CheckBox_GeomMode_Equal.IsChecked ?? false;

            config.AnalyzeBaseLinesOnly = this.CheckBox_UseBaselines.IsChecked ?? false;
            //config.AddPropertyToObject2By1 = this.CheckBox_InsertParameterTo2.IsChecked ?? false;
            config.Tolerance = double.Parse(this.TextBox_Tolerance.Text, CultureInfo.InvariantCulture);
            config.Segmentation = int.Parse(this.TextBox_Segmentation.Text, CultureInfo.InvariantCulture);

            // Типы свойств
            if (this.ListBox_ParametersGroup1.Items.Count > 0 && this.ListBox_ParametersGroup1.SelectedItems.Count > 0)
            {
                List<Guid> propIds = new List<Guid>();
                foreach (ListViewItem item in this.ListBox_ParametersGroup1.Items)
                {
                    Guid itemGuid = (Guid)item.Tag;
                    if (item.IsSelected) propIds.Add(itemGuid);
                }
                if (propIds.Any()) config.PropertiesToCopy = propIds.ToArray();
            }
            return config;
        }

        public void SetConfigToUi(ClashDetectiveConfig? config)
        {
            if (config == null)
            {
                throw new Exception("RengaBri4ka. Конфиг GridSlopeAnalyzingConfig не определен или равен null");
            }

            this.TextBox_ClashName.Text = config.Name;

            setObjectTypesSelected(this.ListBox_Group1, config.Group1);
            setObjectTypesSelected(this.ListBox_Group2, config.Group2);

            //this.CheckBox_GeomMode_Separate.IsChecked = config.ClashSettings.Separate;
            this.CheckBox_GeomMode_Touching.IsChecked = config.ClashSettings.Touching;
            this.CheckBox_GeomMode_Intersecting.IsChecked = config.ClashSettings.Intersecting;
            this.CheckBox_GeomMode_Contains.IsChecked = config.ClashSettings.Contains;
            this.CheckBox_GeomMode_ContainedBy.IsChecked = config.ClashSettings.ContainedBy;
            this.CheckBox_GeomMode_Equal.IsChecked = config.ClashSettings.Equal;

            //this.CheckBox_InsertParameterTo2.IsChecked = config.AddPropertyToObject2By1;
            this.CheckBox_UseBaselines.IsChecked = config.AnalyzeBaseLinesOnly;
            this.TextBox_Tolerance.Text = config.Tolerance.ToString();
            this.TextBox_Segmentation.Text = config.Segmentation.ToString();
        }
        public void Button_SaveSettingsToFile_Click(object sender, RoutedEventArgs e)
        {
            ConfigIO.SaveToWithDialogue(GetConfigFromUI());
        }

        public void Button_LoadSettingsFromFile_Click(object sender, RoutedEventArgs e)
        {
            ClashDetectiveConfig? config = (ClashDetectiveConfig?)ConfigIO.LoadFromWithDialogue<ClashDetectiveConfig>();
            SetConfigToUi(config);
        }
        #endregion

        private RengaTypeInfo[] pRengaTypes;
    }
}

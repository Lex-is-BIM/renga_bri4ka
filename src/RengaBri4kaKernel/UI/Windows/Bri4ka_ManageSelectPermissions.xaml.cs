using Renga;
using System;
using System.Collections.Generic;
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
using RengaBri4kaKernel.RengaInternalResources;

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_ManageSelectPermissions.xaml
    /// </summary>
    public partial class Bri4ka_ManageSelectPermissions : Window
    {
        public Bri4ka_ManageSelectPermissions()
        {
            InitializeComponent();

            this.TextBox_EnumVariants.AcceptsReturn = true;
            this.ListBox_PermittedEnums.SelectionMode = SelectionMode.Multiple;

            RengaManageSelectPermissionsConfig? config = (RengaManageSelectPermissionsConfig?)ConfigIO.LoadFrom<RengaManageSelectPermissionsConfig>(ConfigIO.GetDefaultPath<RengaManageSelectPermissionsConfig>());
            if (config == null) config = new RengaManageSelectPermissionsConfig();
            if (!config.PropertyValues.Contains(RengaManageSelectPermissionsConfig.NoBehaviourName)) config.PropertyValues.Add(RengaManageSelectPermissionsConfig.NoBehaviourName);

            this.AcceptConfig(config);
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void AcceptConfig(RengaManageSelectPermissionsConfig? config)
        {
            if (config == null) return;
            this.TextBox_EnumVariants.Text = string.Join("\n", config.PropertyValues);

            this.ListBox_PermittedEnums.Items.Clear();
            foreach (var prop in config.PropertyValues)
            {
                ListBoxItem propItem = new ListBoxItem();
                propItem.Content = prop;
                propItem.IsSelected = config.AcceptedRoles.Contains(prop);

                this.ListBox_PermittedEnums.Items.Add(propItem);
            }

            if (config.OnSelectMode == BehaviorOnSelectUnaccessObjects.SkipSelectionWithoutExceptions) this.RadioButton_ReselSel.IsChecked = true;
            else if (config.OnSelectMode == BehaviorOnSelectUnaccessObjects.SkipSelectionWithExceptions) this.RadioButton_ReselSelAndShowWarning.IsChecked = true;
            else if (config.OnSelectMode == BehaviorOnSelectUnaccessObjects.ShowSelectionDialog) this.RadioButton_AskEdit.IsChecked = true;
        }

        private RengaManageSelectPermissionsConfig GetConfig()
        {
            RengaManageSelectPermissionsConfig config = new RengaManageSelectPermissionsConfig();
            config.PropertyValues = new List<string>() { RengaManageSelectPermissionsConfig.NoBehaviourName};

            string[] valuesList;
            if (this.TextBox_EnumVariants.Text.Contains('\n'))
            {
                valuesList = this.TextBox_EnumVariants.Text.Split('\n');
            }
            else valuesList = new string[] { this.TextBox_EnumVariants.Text };

            foreach (var propValue in valuesList)
            {
                string propValueEd = propValue.Trim().Replace(@"\r", "");
                if (propValueEd.Length > 1 && !config.PropertyValues.Contains(propValueEd)) config.PropertyValues.Add(propValueEd);
            }

            config.AcceptedRoles = new List<string>();

            foreach (ListBoxItem propItem in this.ListBox_PermittedEnums.Items)
            {
                string propItemCaption = (propItem.Content.ToString() ?? "");

                if (propItem.IsSelected && config.PropertyValues.Contains(propItemCaption)) config.AcceptedRoles.Add(propItemCaption);
            }

            if (this.RadioButton_ReselSel.IsChecked == true) config.OnSelectMode = BehaviorOnSelectUnaccessObjects.SkipSelectionWithoutExceptions;
            else if (this.RadioButton_ReselSelAndShowWarning.IsChecked == true) config.OnSelectMode = BehaviorOnSelectUnaccessObjects.SkipSelectionWithExceptions;
            else if (this.RadioButton_AskEdit.IsChecked == true) config.OnSelectMode = BehaviorOnSelectUnaccessObjects.ShowSelectionDialog;

            return config;

        }

        #region Handlers

        private void OnClick_SaveSettings(object sender, RoutedEventArgs e)
        {
            RengaManageSelectPermissionsConfig config = this.GetConfig();
            ConfigIO.SaveToWithDialogue(config);
        }

        private void OnClick_LoadSettings(object sender, RoutedEventArgs e)
        {
            RengaManageSelectPermissionsConfig? config = (RengaManageSelectPermissionsConfig?)ConfigIO.LoadFromWithDialogue<RengaManageSelectPermissionsConfig>();
            this.AcceptConfig(config);
        }

        private void OnClick_SetSettings(object sender, RoutedEventArgs e)
        {
            RengaManageSelectPermissionsConfig config = this.GetConfig();
            RengaManageSelectPermissions.CreateInstance().SetConfig(config);
            ConfigIO.SaveTo(null, config);

            this.Close();
        }

        #endregion

        ~Bri4ka_ManageSelectPermissions()
        {
            ConfigIO.SaveTo(null, this.GetConfig());
        }
    }
}

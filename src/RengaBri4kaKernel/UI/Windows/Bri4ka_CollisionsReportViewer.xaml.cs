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
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Functions;

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_CollisionsReportViewer.xaml
    /// </summary>
    public partial class Bri4ka_CollisionsReportViewer : Window
    {
        private RengaCollisionsReportViewer pActions;
        public Bri4ka_CollisionsReportViewer()
        {
            InitializeComponent();
            pActions = new RengaCollisionsReportViewer();

            SetConfigToUi(pActions.GetLastReport());
        }

        private void SetConfigToUi(ClashDetectiveReport? config)
        {
            if (config == null) return;

            this.TextBox_ConfigInfo.Text = config.Settings.ToString();
            this.DataGrid_Collisions.Items.Clear();
            foreach (ClashDetectiveReportItem collisionInfo in config.Items)
            {
                this.DataGrid_Collisions.Items.Add(collisionInfo);
            }    
        }

        private void OnSelectCollision()
        {
            ClashDetectiveReportItem? selItem = this.DataGrid_Collisions.SelectedItem as ClashDetectiveReportItem;
            if (selItem == null) return;
            this.TextBox_ItemInfo.Text = selItem.ToString();
            Clipboard.SetText(selItem.ToString());

            Guid[] ids = new Guid[] { selItem.ObjectId1, selItem.ObjectId2 };
            int[]? ids2 = RengaUtils.ConvertUniqueIdsToId(ids);
            if (ids2 == null) return;

            if (this.CheckBox_HideAllOtherObjects.IsChecked == true)
            {
                RengaUtils.EditVisibility(ids2);
            }
            if (this.CheckBox_GoToObjects.IsChecked == true)
            {
                RengaUtils.SetObjectsSelected(ids2);
                //RengaUtils.LookTo(selItem.BBoxMin, selItem.BBoxMin);
            }
        }

        #region Handlers
        private void Button_LoadReport_Click(object sender, RoutedEventArgs e)
        {
            ClashDetectiveReport? config = (ClashDetectiveReport?)ConfigIO.LoadFromWithDialogue<ClashDetectiveReport>(ClashDetectiveReport.GetSavePath());
            SetConfigToUi(config);
        }
        //
        private void CheckBox_GoToObjects_Checked(object sender, RoutedEventArgs e)
        {
            OnSelectCollision();
        }

        private void CheckBox_HideAllOtherObjects_Checked(object sender, RoutedEventArgs e)
        {
            OnSelectCollision();
        }


        private void DataGrid_Collisions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnSelectCollision();
        }

        private void Button_SaveToHTML_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

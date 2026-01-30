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

using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Functions;

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_LinkRoomAndFloor.xaml
    /// </summary>
    public partial class Bri4ka_LinkRoomAndFloor : Window
    {
        public Bri4ka_LinkRoomAndFloor()
        {
            InitializeComponent();

            LinkRoomWithFloorConfig? config = (LinkRoomWithFloorConfig?)ConfigIO.LoadFrom<LinkRoomWithFloorConfig>(ConfigIO.GetDefaultPath<LinkRoomWithFloorConfig>());
            if (config == null) config = new LinkRoomWithFloorConfig();

            SetConfigToUi(config);
            this.SizeToContent = SizeToContent.WidthAndHeight;

        }

        #region Handlers
        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            LinkRoomWithFloorConfig config = this.GetConfigFromUI();
            ConfigIO.SaveTo<LinkRoomWithFloorConfig>(ConfigIO.GetDefaultPath<LinkRoomWithFloorConfig>(), config);

            RengaLinkRoomWithFloor func = new RengaLinkRoomWithFloor();
            func.Calculate(config);

            this.DialogResult = true;
            this.Close();
        }
        #endregion

        #region Config
        public LinkRoomWithFloorConfig GetConfigFromUI()
        {
            LinkRoomWithFloorConfig config = new LinkRoomWithFloorConfig();

            config.UseOnlyVisible = this.CheckBox_UseOnlyVisible.IsChecked ?? false;

            if (this.RadioButton_Geom_Centroid.IsChecked == true) config.RoomGeometryMode = RoomGeometryVariant.Centroid;
            else if (this.RadioButton_Geom_ExtContour.IsChecked == true) config.RoomGeometryMode = RoomGeometryVariant.BaselineContour;
            else if (this.RadioButton_Geom_ExtSolidContour.IsChecked == true) config.RoomGeometryMode = RoomGeometryVariant.SolidsFloorContour;
           
            return config;
        }

        public void SetConfigToUi(LinkRoomWithFloorConfig? config)
        {
            if (config == null)
            {
                RengaUtils.ShowMessageBox("Конфиг LinkRoomWithFloorConfig не определен");
                return;
            }

            this.CheckBox_UseOnlyVisible.IsChecked = config.UseOnlyVisible;

            if (config.RoomGeometryMode == RoomGeometryVariant.Centroid) this.RadioButton_Geom_Centroid.IsChecked = true;
            else if (config.RoomGeometryMode == RoomGeometryVariant.BaselineContour) this.RadioButton_Geom_ExtContour.IsChecked = true;
            else if (config.RoomGeometryMode == RoomGeometryVariant.SolidsFloorContour) this.RadioButton_Geom_ExtSolidContour.IsChecked = true;

        }
        public void Button_SaveSettingsToFile_Click(object sender, RoutedEventArgs e)
        {
            ConfigIO.SaveToWithDialogue(GetConfigFromUI());
        }

        public void Button_LoadSettingsFromFile_Click(object sender, RoutedEventArgs e)
        {
            LinkRoomWithFloorConfig? config = (LinkRoomWithFloorConfig?)ConfigIO.LoadFromWithDialogue<LinkRoomWithFloorConfig>();
            SetConfigToUi(config);
        }
        #endregion
    }
}

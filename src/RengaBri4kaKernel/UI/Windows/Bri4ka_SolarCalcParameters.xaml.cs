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
    /// Interaction logic for Bri4ka_SolarCalcParameters.xaml
    /// </summary>
    public partial class Bri4ka_SolarCalcParameters : Window
    {
        public Bri4ka_SolarCalcParameters()
        {
            InitializeComponent();

            ShadowCalcParametersConfig? config = (ShadowCalcParametersConfig?)ConfigIO.LoadFrom<ShadowCalcParametersConfig>(ConfigIO.GetDefaultPath<ShadowCalcParametersConfig>());
            if (config == null) config = new ShadowCalcParametersConfig();

            SetConfigToUi(config);
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        #region Handlers
        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            ShadowCalcParametersConfig config = this.GetConfigFromUI();
            ConfigIO.SaveTo<ShadowCalcParametersConfig>(ConfigIO.GetDefaultPath<ShadowCalcParametersConfig>(), config);

            RengaShadowsBySunCreator sunParams = new RengaShadowsBySunCreator();
            sunParams.Start(config);

            this.DialogResult = true;
            this.Close();
        }
        #endregion
        #region Config

        public ShadowCalcParametersConfig GetConfigFromUI()
        {
            ShadowCalcParametersConfig config = new ShadowCalcParametersConfig();

            config.Name = this.TextBox_Name.Text;
            config.Latitude = double.Parse(this.TextBox_Latitude.Text, CultureInfo.InvariantCulture);
            config.Longitude = double.Parse(this.TextBox_Longitude.Text, CultureInfo.InvariantCulture);
            config.Date = DateTime.Parse(this.TextBox_DataAnalyze.Text, CultureInfo.InvariantCulture);
            config.GroundElevation = double.Parse(this.TextBox_ElevationGround.Text, CultureInfo.InvariantCulture);
            return config;
        }

        public void SetConfigToUi(ShadowCalcParametersConfig? config)
        {
            if (config == null)
            {
                RengaUtils.ShowMessageBox("Конфиг ShadowCalcParametersConfig не определен");
                return;
            }

            this.TextBox_Name.Text = config.Name;

            this.TextBox_Latitude.Text = config.Latitude.ToString();
            this.TextBox_Longitude.Text = config.Longitude.ToString();
            this.TextBox_DataAnalyze.Text = config.Date.ToString("d");
            this.TextBox_ElevationGround.Text = config.GroundElevation.ToString();

        }
        public void Button_SaveSettingsToFile_Click(object sender, RoutedEventArgs e)
        {
            ConfigIO.SaveToWithDialogue(GetConfigFromUI());
        }

        public void Button_LoadSettingsFromFile_Click(object sender, RoutedEventArgs e)
        {
            ShadowCalcParametersConfig? config = (ShadowCalcParametersConfig?)ConfigIO.LoadFromWithDialogue<ShadowCalcParametersConfig>();
            SetConfigToUi(config);
        }
        #endregion
    }
}

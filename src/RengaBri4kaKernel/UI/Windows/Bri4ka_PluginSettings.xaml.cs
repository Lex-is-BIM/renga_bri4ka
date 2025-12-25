using RengaBri4kaKernel.Configs;
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

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_PluginSettings.xaml
    /// </summary>
    public partial class Bri4ka_PluginSettings : Window
    {
        public Bri4ka_PluginSettings()
        {
            InitializeComponent();
            SetConfig(PluginAppConfig.CreateInstance());

            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void SetConfig(PluginAppConfig? config)
        {
            if (config == null) return;

            this.CheckBox_AutoStartFollowProjectVisualStyle.IsChecked = config.AutoStartFollowProjectVisualStyle;
            switch (config.ActiveProjectVisualStyle)
            {
                case Renga.VisualStyle.VisualStyle_Wireframe:
                    this.RadioButton_3dStyle_Wireframe.IsChecked = true;
                    break;
                case Renga.VisualStyle.VisualStyle_Monochrome:
                    this.RadioButton_3dStyle_Monochrome.IsChecked = true;
                    break;
                case Renga.VisualStyle.VisualStyle_Color:
                    this.RadioButton_3dStyle_Color.IsChecked = true;
                    break;
                case Renga.VisualStyle.VisualStyle_Textured:
                    this.RadioButton_3dStyle_Textured.IsChecked = true;
                    break;
            }
        }

        private void SaveConfig()
        {
            PluginAppConfig.CreateInstance().AutoStartFollowProjectVisualStyle = this.CheckBox_AutoStartFollowProjectVisualStyle.IsChecked ?? false;
            if (this.RadioButton_3dStyle_Wireframe.IsChecked == true) PluginAppConfig.CreateInstance().ActiveProjectVisualStyle = Renga.VisualStyle.VisualStyle_Wireframe;
            else if (this.RadioButton_3dStyle_Monochrome.IsChecked == true) PluginAppConfig.CreateInstance().ActiveProjectVisualStyle = Renga.VisualStyle.VisualStyle_Monochrome;
            else if (this.RadioButton_3dStyle_Color.IsChecked == true) PluginAppConfig.CreateInstance().ActiveProjectVisualStyle = Renga.VisualStyle.VisualStyle_Color;
            else if (this.RadioButton_3dStyle_Textured.IsChecked == true) PluginAppConfig.CreateInstance().ActiveProjectVisualStyle = Renga.VisualStyle.VisualStyle_Textured;

            PluginAppConfig.CreateInstance().Save();
        }

        #region Handlers

        private void Button_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            this.Close();

        }
        #endregion
    }
}

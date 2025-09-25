using RengaBri4kaKernel.Configs;
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

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_TextSettings.xaml
    /// </summary>
    public partial class Bri4ka_TextSettings : Window, IWindowWithConfig<TextSettingsConfig>
    {
        public Bri4ka_TextSettings(TextSettingsConfig? config)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            if (config != null) SetConfigToUi(config);
        }

        private void UpdateColorBorder()
        {
            if (pColorSelected == null) return;
            this.Border_ColorPreview.Background = new SolidColorBrush((Color)pColorSelected); ;
        }


        #region Handlers
        public void Button_ColorEdit_Click(object sender, RoutedEventArgs e)
        {
            Bri4ka_ColorPallette form = new Bri4ka_ColorPallette(ColoringFunctionMode.WidgetToGetColor);
            if (form.ShowDialog() == true)
            {
                this.pColorSelected = form.CreatedColor;
                UpdateColorBorder();
            }
        }

        public void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        #endregion
        #region Config

        public TextSettingsConfig GetConfigFromUI()
        {
            TextSettingsConfig config = new TextSettingsConfig();
            config.FontCapSize = int.Parse(this.TextBox_FontCapSize.Text, CultureInfo.InvariantCulture);
            config.FontName = this.TextBox_FontFamily.Text;
            config.IsBold = this.CheckBox_StyleBold.IsChecked ?? false;
            config.IsItalic = this.CheckBox_StyleItalic.IsChecked ?? false;
            config.IsUnderline = this.CheckBox_StyleUnderline.IsChecked ?? false;
            if (pColorSelected != null) config.FontColorHex = TextSettingsConfig.ToHex((Color)pColorSelected);

            return config;
        }
        public void SetConfigToUi(TextSettingsConfig? config)
        {
            if (config == null)
            {
                throw new Exception("RengaBri4ka. Конфиг TextSettingsConfig не определен или равен null");
            }

            this.TextBox_FontCapSize.Text = config.FontCapSize.ToString();
            this.TextBox_FontFamily.Text = config.FontName;
            this.CheckBox_StyleBold.IsChecked = config.IsBold;
            this.CheckBox_StyleItalic.IsChecked = config.IsItalic;
            this.CheckBox_StyleUnderline.IsChecked = config.IsUnderline;
            pColorSelected = TextSettingsConfig.FromHex(config.FontColorHex);
            UpdateColorBorder();
        }
        public void Button_SaveSettingsToFile_Click(object sender, RoutedEventArgs e)
        {
            ConfigIO.SaveToWithDialogue(GetConfigFromUI());
        }

        public void Button_LoadSettingsFromFile_Click(object sender, RoutedEventArgs e)
        {
            TextSettingsConfig? config = (TextSettingsConfig?)ConfigIO.LoadFromWithDialogue<TextSettingsConfig>();
            SetConfigToUi(config);
        }
        #endregion

        private System.Windows.Media.Color? pColorSelected;
    }
}

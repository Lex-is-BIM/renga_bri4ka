using RengaBri4kaKernel.Functions;
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
    /// Interaction logic for Bri4ka_ColorPallette.xaml
    /// </summary>
    public partial class Bri4ka_ColorPallette : Window
    {
        public Bri4ka_ColorPallette(ColoringFunctionMode mode)
        {
            pMode = mode;
            InitializeComponent();
            UpdateColorPreview();
            //this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        public Color CreatedColor { get; set; }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RedTextBox == null || GreenTextBox == null || BlueTextBox == null || AlphaTextBox == null)
                return;

            // Update text boxes when sliders change
            RedTextBox.Text = ((int)RedSlider.Value).ToString();
            GreenTextBox.Text = ((int)GreenSlider.Value).ToString();
            BlueTextBox.Text = ((int)BlueSlider.Value).ToString();
            AlphaTextBox.Text = ((int)AlphaSlider.Value).ToString();

            UpdateColorPreview();
        }

        private int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Validate and update corresponding slider
                if (int.TryParse(textBox.Text, out int value))
                {
                    value = Clamp(value, 0, 255);

                    switch (textBox.Tag as string)
                    {
                        case "Red":
                            RedSlider.Value = value;
                            break;
                        case "Green":
                            GreenSlider.Value = value;
                            break;
                        case "Blue":
                            BlueSlider.Value = value;
                            break;
                        case "Alpha":
                            AlphaSlider.Value = value;
                            break;
                    }

                    UpdateColorPreview();
                }
            }
        }


        private void UpdateColorPreview()
        {
            if (ColorPreview == null || ColorInfoText == null)
                return;

            try
            {
                byte r = (byte)RedSlider.Value;
                byte g = (byte)GreenSlider.Value;
                byte b = (byte)BlueSlider.Value;
                byte a = (byte)AlphaSlider.Value;

                // Create color with alpha
                Color color = Color.FromArgb(a, r, g, b);

                // Update preview background
                ColorPreview.Background = new SolidColorBrush(color);

                // Update color information
                ColorInfoText.Text = $"RGB: ({r}, {g}, {b})\n" +
                                   $"ARGB: (#{a:X2}{r:X2}{g:X2}{b:X2})\n" +
                                   $"Hex: #{r:X2}{g:X2}{b:X2}\n" +
                                   $"Alpha: {a} ({(a / 255.0 * 100):F1}%)";
            }
            catch (Exception ex)
            {
                ColorInfoText.Text = $"Error: {ex.Message}";
            }
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            byte r = (byte)RedSlider.Value;
            byte g = (byte)GreenSlider.Value;
            byte b = (byte)BlueSlider.Value;
            byte a = (byte)AlphaSlider.Value;

            CreatedColor = Color.FromArgb(a, r, g, b);

            if (pMode == ColoringFunctionMode.ColorSelectedText)
            {
                RengaTextColoring func = new RengaTextColoring();
                func.SetColor(CreatedColor);
            }
            this.Close();
        }

        private ColoringFunctionMode pMode;
    }
}

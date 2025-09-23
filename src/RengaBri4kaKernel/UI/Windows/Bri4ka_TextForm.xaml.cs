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
    /// Interaction logic for Bri4ka_TextForm.xaml
    /// </summary>
    public partial class Bri4ka_TextForm : Window
    {
        public Bri4ka_TextForm()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        public void SetText(string text)
        {
            this.TextBox_TextField.Text = text;
        }
        public static void ShowTextWindow(string text, string? caption)
        {
            Bri4ka_TextForm form = new Bri4ka_TextForm();
            form.SetText(text);
            if (caption != null) form.Title = caption;

            form.ShowDialog();
        }

        #region Handlers
        private void Button_CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.TextBox_TextField.Text);
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}

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

using RengaBri4kaKernel.Functions;

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_CmdPreProcessor.xaml
    /// </summary>
    public partial class Bri4ka_CmdPreProcessor : Window
    {
        private RengaCmdPreProcessor pActions;
        public Bri4ka_CmdPreProcessor()
        {
            InitializeComponent();
            pActions = new RengaCmdPreProcessor();

            this.TextBox_CMD.AcceptsReturn = true;
            this.TextBox_CMD.TextChanged += TextBox_CMD_TextChanged;

            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void RunCommand()
        {
            string text = this.TextBox_CMD.Text;
            if (text.Contains("\n"))
            {
                pActions.RunCommand(text);
                this.TextBox_CMD_Log.Text += text;
                TextBox_CMD.Text = "";
            }
        }

        #region Handlers
        private void TextBox_CMD_TextChanged(object sender, TextChangedEventArgs e)
        {
            RunCommand();
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            this.TextBox_CMD_Log.Text = "";
        }

        private void Button_SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            pActions.SaveScenario(this.TextBox_CMD_Log.Text);
            RunCommand();
        }
        #endregion
    }
}

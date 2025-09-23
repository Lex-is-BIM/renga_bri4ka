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
    /// Interaction logic for Bri4ka_CalcGridsSlopes.xaml
    /// </summary>
    public partial class Bri4ka_CalcGridsSlopes : Window
    {
        private RengaGridSlopeAnalyzing pActions;
        public Bri4ka_CalcGridsSlopes()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            pActions = new RengaGridSlopeAnalyzing();
        }

        private GridSlopeAnalyzingConfig GetConfigFromUI()
        {
            GridSlopeAnalyzingConfig config = new GridSlopeAnalyzingConfig();
            //TODO: read...
            return config;
        }

        private void SetConfigToUi(GridSlopeAnalyzingConfig? config)
        {
            if (config == null)
            {
                throw new Exception("RengaBri4ka. Конфиг RengaGridSlopeAnalyzingConfig не определен или равен null");
            }

            //TODO: set...
        }

        #region Handlers
        private void Button_FontSettingsShow_Click(object sender, RoutedEventArgs e)
        {
            //TODO: create window, config ...
        }

        private void Button_SaveSettingsToFile_Click(object sender, RoutedEventArgs e)
        {
            ConfigIO.SaveToWithDialogue(GetConfigFromUI());
        }

        private void Button_LoadSettingsFromFile_Click(object sender, RoutedEventArgs e)
        {
            GridSlopeAnalyzingConfig? config = (GridSlopeAnalyzingConfig?)ConfigIO.LoadFromWithDialogue<GridSlopeAnalyzingConfig>();
            SetConfigToUi(config);
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            pActions.mConfig = this.GetConfigFromUI();
            pActions.mObjectIds = UserInput.GetSelectedObjects();
            pActions.Calculate();
            this.Close();
        }
        #endregion
    }
}

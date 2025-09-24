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
    /// Interaction logic for Bri4ka_CalcGridsSlopes.xaml
    /// </summary>
    public partial class Bri4ka_CalcGridsSlopes : Window, IWindowWithConfig<GridSlopeAnalyzingConfig>
    {
        private RengaGridSlopeAnalyzing pActions;
        public Bri4ka_CalcGridsSlopes()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            pActions = new RengaGridSlopeAnalyzing();
        }

        #region Handlers
        private void Button_FontSettingsShow_Click(object sender, RoutedEventArgs e)
        {
            Bri4ka_TextSettings settings = new Bri4ka_TextSettings(pTextSettings);
            if (settings.ShowDialog() == true)
            {
                pTextSettings = settings.GetConfigFromUI();
            }
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            pActions.mConfig = this.GetConfigFromUI();
            pActions.mObjectIds = UserInput.GetSelectedObjects();
            pActions.Calculate();
            this.Close();
        }
        #endregion

        #region Config

        public GridSlopeAnalyzingConfig GetConfigFromUI()
        {
            GridSlopeAnalyzingConfig config = new GridSlopeAnalyzingConfig();
            //TODO: read...
            config.IgnoreTrianglesSquareMore = this.CheckBox_IgnoreTrianglesSquareMore.IsChecked ?? false;
            config.IgnoringTrianglesSquareMore = double.Parse(this.TextBox_IgnoreValuesMore.Text, CultureInfo.InvariantCulture);

            config.IgnoreTrianglesSquareLess = this.CheckBox_IgnoreTrianglesSquareLess.IsChecked ?? false;
            config.IgnoringTrianglesSquareLess = double.Parse(this.TextBox_IgnoreValuesLess.Text, CultureInfo.InvariantCulture);

            config.IgnoreValuesMore = this.CheckBox_IgnoreValuesMore.IsChecked ?? false;
            config.IgnoringValuesMore = double.Parse(this.TextBox_IgnoreValuesMore.Text, CultureInfo.InvariantCulture);

            config.IgnoreValuesLess = this.CheckBox_IgnoreValuesLess.IsChecked ?? false;
            config.IgnoringValuesLess = double.Parse(this.TextBox_IgnoreValuesLess.Text, CultureInfo.InvariantCulture);

            config.SaveExtremeResultsToProperties = this.CheckBox_SaveResultsMinMax.IsChecked ?? false;

            if (this.RadioButton_ResultAsPercent.IsChecked == true) config.Units = SlopeResultUnitsVariant.Percent;
            else if (this.RadioButton_ResultAsPromille.IsChecked == true) config.Units = SlopeResultUnitsVariant.Promille;
            else if (this.RadioButton_ResultAsDegrees.IsChecked == true) config.Units = SlopeResultUnitsVariant.Degree;
            else if (this.RadioButton_ResultAsRadians.IsChecked == true) config.Units = SlopeResultUnitsVariant.Radians;

            if (this.pTextSettings != null) config.TextStyle = this.pTextSettings;

            return config;
        }

        public void SetConfigToUi(GridSlopeAnalyzingConfig? config)
        {
            if (config == null)
            {
                throw new Exception("RengaBri4ka. Конфиг GridSlopeAnalyzingConfig не определен или равен null");
            }

            this.CheckBox_IgnoreTrianglesSquareMore.IsChecked = config.IgnoreTrianglesSquareMore;
            this.TextBox_IgnoreValuesMore.Text = config.IgnoringTrianglesSquareMore.ToString();

            this.CheckBox_IgnoreTrianglesSquareLess.IsChecked = config.IgnoreTrianglesSquareLess;
            this.TextBox_IgnoreValuesLess.Text = config.IgnoringTrianglesSquareLess.ToString();

            this.CheckBox_IgnoreValuesMore.IsChecked = config.IgnoreValuesMore;
            this.TextBox_IgnoreValuesMore.Text = config.IgnoringValuesMore.ToString();

            this.CheckBox_IgnoreValuesLess.IsChecked = config.IgnoreValuesLess;
            this.TextBox_IgnoreValuesLess.Text = config.IgnoringValuesLess.ToString();

            this.CheckBox_SaveResultsMinMax.IsChecked = config.SaveExtremeResultsToProperties;

            if (config.Units == SlopeResultUnitsVariant.Percent) this.RadioButton_ResultAsPercent.IsChecked = true;
            else if (config.Units == SlopeResultUnitsVariant.Promille) this.RadioButton_ResultAsPromille.IsChecked = true;
            else if (config.Units == SlopeResultUnitsVariant.Degree) this.RadioButton_ResultAsDegrees.IsChecked = true;
            else if (config.Units == SlopeResultUnitsVariant.Radians) this.RadioButton_ResultAsRadians.IsChecked = true;

            this.pTextSettings = config.TextStyle;
        }
        public void Button_SaveSettingsToFile_Click(object sender, RoutedEventArgs e)
        {
            ConfigIO.SaveToWithDialogue(GetConfigFromUI());
        }

        public void Button_LoadSettingsFromFile_Click(object sender, RoutedEventArgs e)
        {
            GridSlopeAnalyzingConfig? config = (GridSlopeAnalyzingConfig?)ConfigIO.LoadFromWithDialogue<GridSlopeAnalyzingConfig>();
            SetConfigToUi(config);
        }
        #endregion

        private TextSettingsConfig? pTextSettings;
    }
}

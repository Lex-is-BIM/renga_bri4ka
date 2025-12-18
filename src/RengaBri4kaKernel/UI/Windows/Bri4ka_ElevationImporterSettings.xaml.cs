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
using Microsoft.Win32;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Functions;

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_ElevationImporterSettings.xaml
    /// </summary>
    public partial class Bri4ka_ElevationImporterSettings : Window
    {
        private RengaElevationImporter pActions;
        public Bri4ka_ElevationImporterSettings()
        {
            InitializeComponent();
            pActions = new RengaElevationImporter();
            this.ListBox_SurfacesList.SelectionMode = SelectionMode.Single;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void showSurfaces(string[]? names)
        {
            this.ListBox_SurfacesList.Items.Clear();
            if (names == null) return;

            foreach (string name in names)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = name;

                if (names.Length == 1) item.IsSelected = true;
                this.ListBox_SurfacesList.Items.Add(item);
            }
        }

        #region Handlers
        private void OnClick_LoadFromLandXML(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбор LandXML файла";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "LandXML файл (*.XML, *.xml) | *.XML;*.xml";

            string? landXMLFulePath = null;

#if DEBUG
            landXMLFulePath = @"E:\Temp\NN_1896_Demo31.xml";
#else
            if (openFileDialog.ShowDialog() == true)
            {
                landXMLFulePath = openFileDialog.FileName;
            }
#endif
            if (landXMLFulePath == null) return;
            showSurfaces(pActions.SetLandXML(landXMLFulePath));

        }

        private void OnClick_StartImport(object sender, RoutedEventArgs e)
        {
            if (!pActions.IsXml)
            {
                RengaUtils.ShowMessageBox("LandXML файл не был загуржен!", true);
                return;
            }
            RengaElevationImporterSettings settings = new RengaElevationImporterSettings();
            if (double.TryParse(this.TextBox_IsolinesStep.Text, out var step)) settings.IsolinesStep = step;
            settings.UseCoordsOffset = this.CheckBox_UseCoordsOffset.IsChecked ?? false;
            
            //TODO: select from listbox
            foreach (ListBoxItem item in this.ListBox_SurfacesList.Items)
            {
                if (item.IsSelected) settings.SelectedSurfaceName = item.Content.ToString();
            }

#if DEBUG
            //settings.SelectedSurfaceName = "1";
#endif
            if (settings.SelectedSurfaceName == null)
            {
                RengaUtils.ShowMessageBox("Поверхность для импорта не была выбрана!", true);
                return;
            }

            settings.AddExternalBorder = this.CheckBox_AddExternalContour.IsChecked ?? false;

            pActions.Import(settings);

            this.Close();
        }

        private void OnSelect_ShowSurfaceInfo(object sender, SelectionChangedEventArgs e)
        {
            

        }
        #endregion
    }
}

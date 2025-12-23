using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Functions;


namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_ViewPointsManager.xaml
    /// </summary>
    public partial class Bri4ka_ViewPointsManager : Window
    {
        public Bri4ka_ViewPointsManager()
        {
            InitializeComponent();
            mViewPoints = new ObservableCollection<ViewPointDefinition>();

            InitData();

            this.Closed += Bri4ka_ViewPointsManager_Closed;
            this.ListBox_ViewPoints.ItemsSource = mViewPoints;
        }

        private void InitData()
        {
            SetConfig(RengaViewPointsManager.GetCurrentConfig(ref this.pLoadedConfig));

            this.TextBox_NewViewPointName.Text = ViewPointDefinition.NameBase;

            this.ListBox_ImageResolution.SelectionMode = SelectionMode.Single;
            this.ListBox_ImageResolution.Items.Clear();
            string[] imageResolutions = RengaViewPointsManager.GetImageResolutions();
            foreach (string imageResolution in imageResolutions)
            {
                ListBoxItem item = new ListBoxItem() { Content = imageResolution };
                if (imageResolution == RengaViewPointsManager.ResolutionDefault) item.IsSelected = true;
                this.ListBox_ImageResolution.Items.Add(item);
            }

            this.ListBox_ViewPoints.SelectionMode = SelectionMode.Single;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private ViewPointsCollectionConfig? GetConfig()
        {
            ViewPointsCollectionConfig config = new ViewPointsCollectionConfig();
            if (PluginData.Project == null) return null;
            config.TargetProjectId = PluginData.Project.ProjectInfo.UniqueId;

            foreach (var vpRaw in mViewPoints)
            {
                config.ViewPoints.Add(vpRaw);
            }
            return config;
        }

        private void SetConfig(ViewPointsCollectionConfig? config)
        {
            if (config == null) return;
            mViewPoints = new ObservableCollection<ViewPointDefinition>();

            foreach (ViewPointDefinition viewPointDef in config.ViewPoints)
            {
                mViewPoints.Add(viewPointDef);
            }           
        }


        #region Handlers
        private void Bri4ka_ViewPointsManager_Closed(object sender, EventArgs e)
        {
            SaveThis();
            PluginData.windowViewPointsManager = null;
        }

        private void ListBox_ViewPoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            manageViewPoint(ViewPointUpdateMode.goToPoint);
        }

        private void Button_AddViewPoint_Click(object sender, RoutedEventArgs e)
        {
            ViewPointDefinition? viewPointDef = GetNewViewPoint();
            if (viewPointDef == null) return;

            mViewPoints.Add(viewPointDef);

            this.ListBox_ViewPoints.SelectedItem = viewPointDef;
            SaveThis();
        }

        private void Button_DeleteViewPoint_Click(object sender, RoutedEventArgs e)
        {
            if (this.ListBox_ViewPoints.SelectedItem != null)
            {
                this.mViewPoints.Remove((ViewPointDefinition)this.ListBox_ViewPoints.SelectedItem);
            }
            SaveThis();
        }

        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            SaveThis();
            this.mViewPoints.Clear();
            pLoadedConfig = null;
        }

        private void Button_UpdateViewPoint_Click(object sender, RoutedEventArgs e)
        {
            manageViewPoint(ViewPointUpdateMode.update);
            SaveThis();
        }

        private void Button_SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            pLoadedConfig = ConfigIO.SaveToWithDialogue(GetConfig());
        }

        private void Button_LoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            ViewPointsCollectionConfig? config = (ViewPointsCollectionConfig?)ConfigIO.LoadFromWithDialogue2<ViewPointsCollectionConfig>(ref this.pLoadedConfig);
            this.SetConfig(config);
        }

        private void Button_CreateScreens_Click(object sender, RoutedEventArgs e)
        {
            if (this.CheckBox_SaveAllScenes.IsChecked == false) manageViewPoint(ViewPointUpdateMode.saveScreen);
            else
            {
                string imageSize = GetTargetImageRes();
                foreach (var viewPointDef in mViewPoints)
                {
                    RengaViewPointsManager.CreateScreen(viewPointDef, imageSize);
                }
            }

            Bri4ka_TextForm.ShowTextWindow("Изображения сохранены в папку " + RengaViewPointsManager.GetScreensDir(null), null);
        }
        #endregion

        private ViewPointDefinition? GetNewViewPoint()
        {
            ViewPointDefinition viewPointDef = new ViewPointDefinition();
            viewPointDef.Name = this.TextBox_NewViewPointName.Text;
            if (viewPointDef.Name == "") viewPointDef.Name = ViewPointDefinition.NameBase;
            viewPointDef.Name += $"_{mViewPoints.Count}";

            Renga.ICamera3D? camera = PluginData.rengaApplication.GetCamera();
            if (camera == null) return null;

            ViewPointCameraParameters? cameraInfo = camera.GetCameraParameters();
            if (cameraInfo == null) return null;
            viewPointDef.Camera = cameraInfo;

            Renga.IModelView mView = PluginData.rengaApplication.ActiveView as Renga.IModelView;
            viewPointDef.ActiveStyle = mView.VisualStyle;

            if (this.RadioButton_ObjectsAll.IsChecked == true) viewPointDef.VisibleObjects = null;
            else
            {
                var visObjects = PluginData.Project.Model.GetAllObjects(this.RadioButton_ObjectsVisibleNow.IsChecked ?? false);
                if (visObjects == null) return null;
                viewPointDef.VisibleObjects = visObjects.Select(ent => ent.Id).ToArray();
            }

            return viewPointDef;
        }

        private void manageViewPoint(ViewPointUpdateMode mode)
        {
            ViewPointDefinition? viewPointDef = this.ListBox_ViewPoints.SelectedItem as ViewPointDefinition;
            if (viewPointDef == null) return;

            if (mode == ViewPointUpdateMode.goToPoint)
            {
                if (this.CheckBox_GoToPoint.IsChecked == true)
                {
                    RengaViewPointsManager.GoToViewPoint(viewPointDef);
                }
                this.TextBox_NewViewPointName.Text = viewPointDef.Name;
            }
            else if (mode == ViewPointUpdateMode.update)
            {
                ViewPointDefinition? editedItem = GetNewViewPoint();
                if (editedItem == null) return;
                // Имя также переопределяют, так что не будем трогать
                if (this.TextBox_NewViewPointName.Text != "") editedItem.Name = this.TextBox_NewViewPointName.Text;
                //editedItem.Name = viewPointDef.Name;

                this.mViewPoints[this.ListBox_ViewPoints.SelectedIndex] = editedItem;
            }
            else if (mode == ViewPointUpdateMode.saveScreen)
            {
                RengaViewPointsManager.CreateScreen(viewPointDef, GetTargetImageRes());
            }
        }

        private string GetTargetImageRes()
        {
            string targetImageRes = RengaViewPointsManager.ResolutionDefault;
            int selIndex = this.ListBox_ImageResolution.SelectedIndex;
            if (selIndex != -1 && selIndex<RengaViewPointsManager.GetImageResolutions().Length) targetImageRes = RengaViewPointsManager.GetImageResolutions()[selIndex];
            return targetImageRes;
        }

        private void SaveThis()
        {
            if (pLoadedConfig == null) return;
            ConfigIO.SaveTo(pLoadedConfig, GetConfig());
        }

        private ObservableCollection<ViewPointDefinition> mViewPoints;
        private string? pLoadedConfig = null;
    }

    enum ViewPointUpdateMode
    {
        //delete,
        update,
        goToPoint,
        saveScreen,
    }
}

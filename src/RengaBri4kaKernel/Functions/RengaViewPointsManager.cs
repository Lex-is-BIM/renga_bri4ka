using Microsoft.SqlServer.Server;
using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RengaBri4kaKernel.Functions
{
    internal class RengaViewPointsManager
    {
        public static string GetCurrentProjectId()
        {
            Guid currentProjectId = PluginData.Project.ProjectInfo.UniqueId;
            return currentProjectId.ToString("N");
        }

        public static string GetScreensDir(string? configName)
        {
            if (configName == null) configName = GetCurrentProjectId();
            string baseDirPath = Path.Combine(PluginConfig.GetDirectoryPath(), "Screens");
            if (!Directory.Exists(baseDirPath)) Directory.CreateDirectory(baseDirPath);

            string screensDir = Path.Combine(baseDirPath, configName);
            if (!Directory.Exists(screensDir)) Directory.CreateDirectory(screensDir);

            return screensDir;
        }

        public static ViewPointsCollectionConfig GetCurrentConfig(ref string path)
        {
            Guid currentProjectId = PluginData.Project.ProjectInfo.UniqueId;
            string configsDir = Path.GetDirectoryName(PluginConfig.GetDefaultPath<ViewPointsCollectionConfig>());

            path = Path.Combine(configsDir, GetCurrentProjectId() + ".xml");
            if (!Directory.Exists(configsDir))
            {
                Directory.CreateDirectory(configsDir);
                return new ViewPointsCollectionConfig();
            }

            ViewPointsCollectionConfig? lastEditedConfig = new ViewPointsCollectionConfig();
            DateTime lastEditedTime = DateTime.Parse("2010/10/12 12:00:00");
            foreach (string configPath in Directory.GetFiles(configsDir, "*.xml", SearchOption.TopDirectoryOnly))
            {
                ViewPointsCollectionConfig? config = (ViewPointsCollectionConfig?)ConfigIO.LoadFrom<ViewPointsCollectionConfig>(configPath);
                if (config == null) continue;
                if (config.TargetProjectId == currentProjectId)
                {
                    var currentEditedTime = new FileInfo(configPath).LastWriteTime;
                    if (lastEditedTime.CompareTo(currentEditedTime) < 0)
                    {
                        lastEditedTime = currentEditedTime;
                        lastEditedConfig = config;
                    }
                }
            }
            return lastEditedConfig;
        }

        // Задание состава объектов сцены и положения камеры
        public static void GoToViewPoint(ViewPointDefinition? viewPointDef)
        {
            if (viewPointDef == null) return;

            Renga.IView view = PluginData.rengaApplication.ActiveView;
            var modelView = view as Renga.IModelView;
            if (modelView == null) return;

            modelView.SetObjectsVisibility2(ObjectsVisibilityVariant.ShowOnlySelected, viewPointDef.VisibleObjects);
            modelView.VisualStyle = viewPointDef.ActiveStyle;

            Renga.ICamera3D? camera = PluginData.rengaApplication.GetCamera();
            if (camera == null) return;
            camera.SetCameraParameters(viewPointDef.Camera);
        }
        public static void CreateScreen(ViewPointDefinition? viewPointDef, string cameraResolution)
        {
            if (viewPointDef == null) return;
            GoToViewPoint(viewPointDef);
            // Создание скриншота
            int image_width = int.Parse(cameraResolution.Split('x')[0]);
            int image_height = int.Parse(cameraResolution.Split('x')[1]);

            Renga.IView view = PluginData.rengaApplication.ActiveView;
            Renga.IScreenshotService? serv = view as Renga.IScreenshotService;
            if (serv == null) return;
            Renga.IScreenshotSettings settings = serv.CreateSettings();
            settings.Width = image_width;
            settings.Height = image_height;

            Renga.IImage image = serv.MakeScreenshot(settings);

            Renga.ImageFormat image_format = Renga.ImageFormat.ImageFormat_PNG;

            string screenSavePath = Path.Combine(GetScreensDir(null), $"{viewPointDef.Name}-cameraResolution.png");
            if (File.Exists(screenSavePath)) File.Delete(screenSavePath);
            image.SaveToFile(screenSavePath, image_format);
        }



        public static string[] GetImageResolutions()
        {
            return new string[]
            {
                "320x240", "352x240", "352x288", "400x240", "480x576", "640x240", "320x480", "640x360", "640x480", "800x480", "800x600", "848x480", "960x540", "1024x600", "1024x768", "1152x864", "1200x600", "1280x720", "1280x768", "1280x1024", "1440x900", "1400x1050", "1536x960", "1536x1024", "1600x900", "1600x1024", "1600x1200", "1680x1050", "1920x1080", "1920x1200", "2048x1080", "2048x1152", "2048x1536", "2560x1440", "2560x1600", "2560x2048", "3072x1620", "3200x1800", "3200x2048", "3200x2400", "3440x1440", "3840x2400", "3840x2160", "4096x2160", "5120x1440", "5120x2700", "5120x4096", "6144x3240", "6400x4096", "6400x4800", "7168x3780", "7680x4320", "7680x4800", "8192x4320"
            };
        }

        public const string ResolutionDefault = "1280x1024";
    }
}

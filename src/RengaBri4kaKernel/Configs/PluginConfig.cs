using Renga;
using RengaBri4kaKernel.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RengaBri4kaKernel.Configs
{
    /// <summary>
    /// Описывает настройки плагина, пути к данным
    /// </summary>
    public class PluginConfig : ConfigIO
    {
        public static string GetDirectoryPath()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Renga Bri4ka Plugin");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
        public static string GetConfigPath()
        {
            return Path.Combine(GetDirectoryPath(), "RengaBri4kaConfig.xml");
        }

        public static void Initialize()
        {
            /*
#if DEBUG
            PluginData.PluginConfig = new PluginConfig();
            PluginData.PluginConfig.RengaVersion = new Version("8.8.22219.0");
#else
            bool needInit = false;
            if (File.Exists(GetConfigPath()))
            {
                var config = ConfigIO.LoadFrom<PluginConfig>(GetConfigPath());
                if (config == null) needInit = true;
                else
                {
                    PluginConfig? config2 = config as PluginConfig;
                    if (config2 == null) needInit = true;
                    else
                    {
                        PluginData.PluginConfig = config2;
                    }
                }
            }
            if (!needInit)
            {
                PluginData.PluginConfig = new PluginConfig();

                //если текущий проект был сохранен, то фиксируем его путь
                string currentProjectPath = PluginData.rengaApplication.Project.FilePath;
                //если есть несохраненные изменения, то сохраняем
                if (PluginData.rengaApplication.Project.HasUnsavedChanges()) PluginData.rengaApplication.Project.Save();

                string tmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                var stat1 = PluginData.rengaApplication.CreateProject();
                var stat2 = PluginData.rengaApplication.Project.SaveAs(tmpPath, ProjectType.ProjectType_Project, true);
                PluginData.rengaApplication.CloseProject(true);

                if (File.Exists(currentProjectPath)) PluginData.rengaApplication.OpenProject(currentProjectPath);

                RengaFileExplorer rengaVersionChecker = new RengaFileExplorer(tmpPath);
                PluginData.PluginConfig.RengaVersionStr = rengaVersionChecker.GetLongVersion();
                File.Delete(tmpPath);

                PluginData.PluginConfig.RengaVersion = new Version(PluginData.PluginConfig.RengaVersionStr);
                ConfigIO.SaveTo<PluginConfig>(PluginConfig.GetConfigPath(), PluginData.PluginConfig);
            }
#endif
            */
        }


        public string? RengaVersionStr { get; set; }

        [XmlIgnore]
        public Version? RengaVersion { get; private set; }
    }
}

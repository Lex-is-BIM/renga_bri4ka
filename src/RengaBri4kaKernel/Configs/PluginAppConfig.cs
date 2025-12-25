using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RengaBri4kaKernel.RengaInternalResources;

namespace RengaBri4kaKernel.Configs
{
    public class PluginAppConfig
    {
        public PluginAppConfig()
        {
            AutoStartFollowProjectVisualStyle = true;
            ActiveProjectVisualStyle = Renga.VisualStyle.VisualStyle_Color;
//            AutoStartFollowDeletingEntities = false;
//#if DEBUG
//            AutoStartFollowDeletingEntities = true;
//#endif
//            FollowDeletingEntitiesIds = new Guid[] { RengaObjectTypes.Level };
        }

        public static string GetConfigPath()
        {
            return Path.Combine(PluginConfig.GetDirectoryPath(), "PluginAppConfig.xml");
        }

        public static PluginAppConfig CreateInstance()
        {
            if (mInstance == null) 
            {
                PluginAppConfig? config = (PluginAppConfig?)ConfigIO.LoadFrom<PluginAppConfig>(GetConfigPath());
                if (config == null) mInstance = new PluginAppConfig();
                else mInstance = config;
            }
            return mInstance;
        }

        /// <summary>
        /// Флаг, задавать ли визуальный стиль всем открываемым и создавваемым проектам при запуске Renga
        /// См. RengaFollowProjectVisualStyle
        /// </summary>
        public bool AutoStartFollowProjectVisualStyle { get; set; }
        public Renga.VisualStyle ActiveProjectVisualStyle { get; set; }

        /// <summary>
        /// Флаг, отслеживать ли удаление Пользователем объектов
        /// </summary>
        //public bool AutoStartFollowDeletingEntities { get; set; }
        //public Guid[]? FollowDeletingEntitiesIds { get; set; }

        public void Save()
        {
            ConfigIO.SaveTo(GetConfigPath(), this);
        }

        private static PluginAppConfig? mInstance = null;
    }
}

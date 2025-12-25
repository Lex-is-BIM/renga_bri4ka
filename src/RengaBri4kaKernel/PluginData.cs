using Renga;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel
{
    /// <summary>
    /// Данный класс наследуют все классы, содержащие публичные поля с static Guid -- идентификаторы свойств, создаваемые плагином
    /// </summary>
    public abstract class PluginParametersCollection
    {
        //TODO: через Reflection получить все дочерние классы и у них забрать статические поля с Guid
        public static Guid[] GetAllParameters()
        {
            var ids = typeof(Renga.EntityTypes).GetRuntimeFields();
            Guid[] guids = new Guid[ids.Count()];
            for (int i = 0; i < ids.Count(); i++)
            {
                FieldInfo field = ids.ElementAt(i);
                var startIdx = field.Name.IndexOf('<');
                var endIdx = field.Name.IndexOf('>');
                guids[i] = (Guid)field.GetValue(null)!;
            }
            return guids;
        }
    }

    public static class PluginData
    {
        public static Renga.Application? rengaApplication;

        public static Renga.IProject? Project
        {
            get
            {
                if (rengaApplication.Project != null) return rengaApplication.Project;
                return null;
            }
        }

        public static Renga.IModel? GetModel()
        {
            Renga.IModel? model = null;
            if (rengaApplication.ActiveView.Type == ViewType.ViewType_View3D || rengaApplication.ActiveView.Type == ViewType.ViewType_Level)
                model = rengaApplication.Project.Model;
            else if (rengaApplication.ActiveView.Type == ViewType.ViewType_Assembly)
            {
                var representedId = (rengaApplication.ActiveView as IModelView).RepresentedEntityId;
                model = rengaApplication.Project.Assemblies.GetById(representedId) as IModel;
            }
            else if (rengaApplication.ActiveView.Type == ViewType.ViewType_Drawing)
            {
                var representedId = (rengaApplication.ActiveView as IModelView).RepresentedEntityId;
                model = rengaApplication.Project.Drawings2.GetById(representedId) as IModel;
            }
            return model;
        }

        public static Version RengaVersion { get; set; } = new Version(1, 0);

        public static string RengaSessionId { get; set; } = "";

        //public static PluginConfig? PluginConfig;
        public static string? PluginFolder;

        public static bool IsRengaProfeccional = false;

        public static Bri4ka_ViewCube? windowViewCube;
        public static Bri4ka_CollisionsReportViewer? windowCollisionReportsViewer;
        public static Bri4ka_CmdPreProcessor? windowCmdPreProcessor;
        public static Bri4ka_ViewPointsManager? windowViewPointsManager;
    }
}

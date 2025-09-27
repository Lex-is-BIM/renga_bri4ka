using RengaBri4kaKernel.Functions;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.AuxFunctions
{
    internal class RengaUtils
    {
        public static void RegisterPropertyIfNotReg(Guid propId, string propName, Renga.PropertyType propType = Renga.PropertyType.PropertyType_Double)
        {
            if (PluginData.Project == null) return;
            if (!PluginData.Project.PropertyManager.IsPropertyRegistered(propId))
            {
                Renga.PropertyDescription propDescr = new Renga.PropertyDescription() { Name = propName, Type = propType };
                PluginData.Project.PropertyManager.RegisterProperty(propId, propDescr);
            }
        }

        public static void AssignPropertiesToTypes(Guid propId, Guid[]? objectTypes)
        {
            if (PluginData.Project == null) return;
            var project = PluginData.Project;

            if (!project.PropertyManager.IsPropertyRegistered(propId)) return;

            if (objectTypes == null) objectTypes = RengaObjectTypes.GetAll();

            foreach (var type in objectTypes)
            {
                try
                {
                    project.PropertyManager.AssignPropertyToType(propId, type);
                }
                catch (Exception ex) { }
            }
        }

        public static RengaTypeInfo[] GetRengaObjectTypes()
        {
            var ids = typeof(Renga.ObjectTypes).GetRuntimeFields();
            RengaTypeInfo[] rengaTypesInfo = new RengaTypeInfo[ids.Count()];
            for (int i = 0; i < ids.Count(); i++)
            {
                FieldInfo field = ids.ElementAt(i);
                var startIdx = field.Name.IndexOf('<');
                var endIdx = field.Name.IndexOf('>');
                rengaTypesInfo[i] = new RengaTypeInfo() { Id = (Guid)field.GetValue(null)!, Name = field.Name.Substring(startIdx + 1, endIdx - 1) };
            }
            return rengaTypesInfo;
        }

    }

    internal class RengaTypeInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}

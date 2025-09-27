using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.AuxFunctions
{
    internal class RengaPropertiesUtils
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

        public static PropertyInfo[]? GetPropertiesInfoByTypes(Guid[] ObjectTypes)
        {
            if (PluginData.Project == null) return null;
            Renga.IPropertyManager propManager = PluginData.Project.PropertyManager;
            List<PropertyInfo> properties = new List<PropertyInfo>();

            for (int propIndex = 0;  propIndex < propManager.PropertyCount; propIndex++)
            {
                Guid propGuid = propManager.GetPropertyId(propIndex);
                Renga.PropertyDescription propDef = propManager.GetPropertyDescription(propGuid);

                bool isNeed = false;
                foreach (Guid t in ObjectTypes)
                {
                    if (propManager.IsPropertyAssignedToType(propGuid, t))
                    {
                        isNeed = true;
                        break;
                    }
                }

                if (isNeed) properties.Add(new PropertyInfo() { Id = propGuid, Name = propDef.Name });
            }

            if (properties.Any()) properties = properties.OrderBy(p => p.Name).ToList();
            return properties.OrderBy(p => p.Name).ToArray();
        }


    }

    class PropertyInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}

using RengaBri4kaKernel.Functions;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}

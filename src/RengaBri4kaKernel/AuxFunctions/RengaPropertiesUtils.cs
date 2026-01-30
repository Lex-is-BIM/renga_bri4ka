using Renga.GridTypes;
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
        public static void RegisterPropertyIfNotReg(Guid propId, string propName, Renga.PropertyType propType = Renga.PropertyType.PropertyType_Double, string[]? enumValues = null)
        {
            if (PluginData.Project == null) return;
            if (!PluginData.Project.PropertyManager.IsPropertyRegistered(propId))
            {
                Renga.IPropertyDescription propDescr = PluginData.Project.PropertyManager.CreatePropertyDescription(propName,
                    propType);
                if (propType == Renga.PropertyType.PropertyType_Enumeration && enumValues != null) propDescr.SetEnumerationItems(enumValues);
                PluginData.Project.PropertyManager.RegisterProperty2(propId, propDescr);
            }
        }

        public static string[] GetEnumerationValues(Guid propId)
        {
            if (PluginData.Project == null) return new string[] { };
            var propDescr = PluginData.Project.PropertyManager.GetPropertyDescription2(propId);
            return propDescr.GetEnumerationItems().Cast<string>().ToArray();
        }

        public static void SetEnumerationValues(Guid propId, string[] values, Dictionary<string, string> valuesMap)
        {
            if (PluginData.Project == null) return;
            if (PluginData.Project.PropertyManager.IsPropertyRegistered(propId))
            {
                List<string> valuesMap2 = new List<string>();
                foreach (var valueMapItem in valuesMap)
                {
                    valuesMap2.Add(valueMapItem.Key);
                    valuesMap2.Add(valueMapItem.Value);
                }
                PluginData.Project.PropertyManager.UpdateEnumValues(propId, values, valuesMap2.ToArray());
            }
        }

        public static void AssignPropertiesToTypes(Guid propId, Guid[]? objectTypes)
        {
            if (PluginData.Project == null) return;
            var project = PluginData.Project;

            if (!project.PropertyManager.IsPropertyRegistered(propId)) return;

            if (objectTypes == null) objectTypes = RengaObjectTypes.GetAll().Select(a=>a.Item1).ToArray();

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

        public static double? GetQuantityValue(Renga.IQuantity rengaQuantity)
        {
            double? propValue = null;

            switch (rengaQuantity.Type)
            {
                case Renga.QuantityType.QuantityType_Count:
                    propValue = rengaQuantity.AsCount() * 1.0;
                    break;
                case Renga.QuantityType.QuantityType_Length:
                    propValue = rengaQuantity.AsLength(Renga.LengthUnit.LengthUnit_Meters);
                    break;
                case Renga.QuantityType.QuantityType_Mass:
                    propValue = rengaQuantity.AsMass(Renga.MassUnit.MassUnit_Kilograms);
                    break;
                case Renga.QuantityType.QuantityType_Area:
                    propValue = rengaQuantity.AsArea(Renga.AreaUnit.AreaUnit_Meters2);
                    break;
                case Renga.QuantityType.QuantityType_Volume:
                    propValue = rengaQuantity.AsVolume(Renga.VolumeUnit.VolumeUnit_Meters3);
                    break;
            }
            return propValue;
        }

        

    }

    class PropertyInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}

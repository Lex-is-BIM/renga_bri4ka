using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{
    internal static class PropertyContainerExtension
    {
        public static Dictionary<string, string> GetPropertiesStr(this Renga.IPropertyContainer propContainer)
        {
            Renga.IGuidCollection rengaPropsIds = propContainer.GetIds();
            Dictionary<string, string> stringConvertedProperties = new Dictionary<string, string>();
            for (int PropIdIndex = 0; PropIdIndex < rengaPropsIds.Count; PropIdIndex++)
            {
                Guid rengaPropId = rengaPropsIds.Get(PropIdIndex);
                Renga.IProperty rengaProp = propContainer.Get(rengaPropId);


                object? propValue = rengaProp.GetPropertyValue();
                if (propValue == null) continue;
                string propValueStr = propValue?.ToString() ?? "";

                stringConvertedProperties.Add(rengaProp.Name, propValueStr);
            }

            return stringConvertedProperties;
        }

        public static void SetProperties(this Renga.IPropertyContainer propContainer, Guid[]? propsId, object?[]? propsData)
        {
            if (propsId == null || propsData == null) return;
            if (PluginData.Project == null) return;

            for (int propCounter = 0; propCounter < propsId.Length; propCounter++)
            {
                Guid propId = propsId[propCounter];
                object? propData = propsData[propCounter];
                //var propDef = PluginData.Project.PropertyManager.GetPropertyDescription(propId);

                if (propContainer.Contains(propId) && propData != null)
                {
                    Renga.IProperty? propInfo = propContainer.Get(propId);
                    propInfo.SetPropertyValue(propData);
                }
            }
        }
    }
}

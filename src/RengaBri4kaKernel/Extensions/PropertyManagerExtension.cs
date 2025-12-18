using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{
    /// <summary>
    /// Определение свойства в Renga
    /// </summary>
    public struct RengaPropertyDefinition
    {
        public Guid Id;
        public string Name;
        public Renga.PropertyType PType;
        public string[]? PropEnums;


        public RengaPropertyDefinition(Guid id, string name, Renga.PropertyType pType, string[]? propEnums)
        {
            this.Id = id;
            this.Name = name;
            this.PType = pType;
            this.PropEnums = propEnums;
        }
    }
    internal static class PropertyManagerExtension
    {
        public static RengaPropertyDefinition[]? GetPropertiesDef(this Renga.IPropertyManager manager)
        {
            if (manager == null) return null;
            RengaPropertyDefinition[] result = new RengaPropertyDefinition[manager.PropertyCount];
            for (int propIndex = 0; propIndex < manager.PropertyCount; propIndex++)
            {
                Guid propId = manager.GetPropertyId(propIndex);
                Renga.IPropertyDescription propDescr = manager.GetPropertyDescription2(propId);

                string[]? propEnums = null;
                if (propDescr.Type == Renga.PropertyType.PropertyType_Enumeration) propEnums = propDescr.GetEnumerationItems().Cast<string>().ToArray();

                result[propIndex] = new RengaPropertyDefinition(propId, propDescr.Name, propDescr.Type, propEnums);
            }
            return result;
        }
    }
}

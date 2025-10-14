using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{
    internal static class PropertyExtension
    {

        public static void SetPropertyValue(this Renga.IProperty? property, object propData)
        {
            if (property == null) return;
            var propDef = PluginData.Project.PropertyManager.GetPropertyDescription(property.Id);
            
            switch (propDef.Type)
            {
                case Renga.PropertyType.PropertyType_Double:
                case Renga.PropertyType.PropertyType_Angle:
                case Renga.PropertyType.PropertyType_Length:
                case Renga.PropertyType.PropertyType_Mass:
                case Renga.PropertyType.PropertyType_Volume:
                    {
                        double? propDataDouble = propData as double?;
                        if (propDataDouble != null)
                        {

                            switch (propDef.Type)
                            {
                                case Renga.PropertyType.PropertyType_Double:
                                    property.SetDoubleValue((double)propDataDouble);
                                    break;
                                case Renga.PropertyType.PropertyType_Angle:
                                    property.SetAngleValue((double)propDataDouble, Renga.AngleUnit.AngleUnit_Degrees);
                                    break;
                                case Renga.PropertyType.PropertyType_Length:
                                    property.SetLengthValue((double)propDataDouble, Renga.LengthUnit.LengthUnit_Meters);
                                    break;
                                case Renga.PropertyType.PropertyType_Mass:
                                    property.SetMassValue((double)propDataDouble, Renga.MassUnit.MassUnit_Kilograms);
                                    break;
                                case Renga.PropertyType.PropertyType_Volume:
                                    property.SetVolumeValue((double)propDataDouble, Renga.VolumeUnit.VolumeUnit_Meters3);
                                    break;
                            }
                        }
                        break;
                    }
                case Renga.PropertyType.PropertyType_Boolean:
                case Renga.PropertyType.PropertyType_Logical:
                    {
                        bool? propDataBoolean = propData as bool?;
                        if (propDataBoolean == null)
                        {
                            if (propDef.Type == Renga.PropertyType.PropertyType_Logical) property.SetLogicalValue(Renga.Logical.Logical_Indeterminate);
                        }
                        else
                        {
                            if (propDef.Type == Renga.PropertyType.PropertyType_Logical)
                            {
                                if (propDataBoolean == true) property.SetLogicalValue(Renga.Logical.Logical_True);
                                else property.SetLogicalValue(Renga.Logical.Logical_False);
                            }
                            else property.SetBooleanValue((bool)propDataBoolean);
                        }
                        break;
                    }
                case Renga.PropertyType.PropertyType_Integer:
                    {
                        int? propDataInteger = propData as int?;
                        if (propDataInteger != null)
                        {
                            property.SetIntegerValue((int)propDataInteger);
                        }
                    }
                    break;
                case Renga.PropertyType.PropertyType_String:
                case Renga.PropertyType.PropertyType_Enumeration:
                    {
                        string? propDataString = propData as string;
                        if (propDataString != null)
                        {
                            if (propDef.Type == Renga.PropertyType.PropertyType_String) property.SetStringValue((string)propDataString);
                            else property.SetEnumerationValue((string)propDataString);
                        }
                    }
                    break;
            }
        }

        public static object? GetPropertyValue(this Renga.IProperty _property)
        {
            object? propValue = null;

            switch (_property.Type)
            {
                case Renga.PropertyType.PropertyType_Angle:
                    propValue = _property.GetAngleValue(Renga.AngleUnit.AngleUnit_Degrees);
                    break;
                case Renga.PropertyType.PropertyType_Area:
                    propValue = _property.GetAreaValue(Renga.AreaUnit.AreaUnit_Meters2);
                    break;
                case Renga.PropertyType.PropertyType_Boolean:
                    propValue = _property.GetBooleanValue();
                    break;
                case Renga.PropertyType.PropertyType_Double:
                    propValue = _property.GetDoubleValue();
                    break;
                case Renga.PropertyType.PropertyType_Enumeration:
                    propValue = _property.GetEnumerationValue();
                    break;
                case Renga.PropertyType.PropertyType_Integer:
                    propValue = _property.GetIntegerValue();
                    break;
                case Renga.PropertyType.PropertyType_Length:
                    propValue = _property.GetLengthValue(Renga.LengthUnit.LengthUnit_Meters);
                    break;
                case Renga.PropertyType.PropertyType_Logical:
                    propValue = _property.GetLogicalValue();
                    break;
                case Renga.PropertyType.PropertyType_Mass:
                    propValue = _property.GetMassValue(Renga.MassUnit.MassUnit_Kilograms);
                    break;
                case Renga.PropertyType.PropertyType_String:
                    propValue = _property.GetStringValue();
                    break;
                case Renga.PropertyType.PropertyType_Volume:
                    propValue = _property.GetVolumeValue(Renga.VolumeUnit.VolumeUnit_Meters3);
                    break;
            }

            return propValue;
        }
    }
}

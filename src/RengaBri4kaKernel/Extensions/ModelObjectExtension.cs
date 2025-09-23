using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{
    internal static class ModelObjectExtension
    {
        public static void SetObjectsProperties(this Renga.IModelObject rengaObject, Guid[]? propsId, object[]? propsData)
        {
            if (propsId == null || propsData == null) return;
            if (PluginData.Project == null) return;
            var propsManager = rengaObject.GetProperties();
            if (propsManager == null) return;

            //var editOperation = PluginData.Project.CreateOperation();
            //editOperation.Start();

            for (int propCounter = 0; propCounter < propsId.Length; propCounter++)
            {
                Guid propId = propsId[propCounter];
                object propData = propsData[propCounter];
                var propDef = PluginData.Project.PropertyManager.GetPropertyDescription(propId);

                if (propsManager.Contains(propId) && propData != null)
                {
                    Renga.IProperty? propInfo = propsManager.Get(propId);

                    if (propInfo == null) continue;
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
                                            propInfo.SetDoubleValue((double)propDataDouble);
                                            break;
                                        case Renga.PropertyType.PropertyType_Angle:
                                            propInfo.SetAngleValue((double)propDataDouble, Renga.AngleUnit.AngleUnit_Degrees);
                                            break;
                                        case Renga.PropertyType.PropertyType_Length:
                                            propInfo.SetLengthValue((double)propDataDouble, Renga.LengthUnit.LengthUnit_Meters);
                                            break;
                                        case Renga.PropertyType.PropertyType_Mass:
                                            propInfo.SetMassValue((double)propDataDouble, Renga.MassUnit.MassUnit_Kilograms);
                                            break;
                                        case Renga.PropertyType.PropertyType_Volume:
                                            propInfo.SetVolumeValue((double)propDataDouble, Renga.VolumeUnit.VolumeUnit_Meters3);
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
                                    if (propDef.Type == Renga.PropertyType.PropertyType_Logical) propInfo.SetLogicalValue(Renga.Logical.Logical_Indeterminate); 
                                }
                                else
                                {
                                    if (propDef.Type == Renga.PropertyType.PropertyType_Logical)
                                    {
                                        if (propDataBoolean == true) propInfo.SetLogicalValue(Renga.Logical.Logical_True);
                                        else propInfo.SetLogicalValue(Renga.Logical.Logical_False);
                                    }
                                    else propInfo.SetBooleanValue((bool)propDataBoolean);
                                }
                                break;
                            }
                        case Renga.PropertyType.PropertyType_Integer:
                            {
                                int? propDataInteger = propData as int?;
                                if (propDataInteger !=null)
                                {
                                    propInfo.SetIntegerValue((int)propDataInteger);
                                }
                            }
                            break;
                        case Renga.PropertyType.PropertyType_String:
                        case Renga.PropertyType.PropertyType_Enumeration:
                            {
                                string? propDataString = propData as string;
                                if (propDataString != null)
                                {
                                    if (propDef.Type == Renga.PropertyType.PropertyType_String) propInfo.SetStringValue((string)propDataString);
                                    else propInfo.SetEnumerationValue((string)propDataString);
                                }
                            }
                            break;
                    }
                }
            }
        }

        public static Renga.IExportedObject3D? GetExportedObject3D(this Renga.IModelObject rengaObject)
        {
            if (PluginData.Project == null) return null;
            Renga.IDataExporter rengaDataExporter = PluginData.Project.DataExporter;
            Renga.IExportedObject3DCollection rengaExportedObject3DCollection = rengaDataExporter.GetObjects3D();

            for (int rengaObjectGeometryCounter = 0; rengaObjectGeometryCounter < rengaExportedObject3DCollection.Count; rengaObjectGeometryCounter++)
            {
                Renga.IExportedObject3D rengaObjectGeometry = rengaExportedObject3DCollection.Get(rengaObjectGeometryCounter);
                if (rengaObjectGeometry.ModelObjectId == rengaObject.Id) return rengaObjectGeometry;
            }
            return null;
        }
    }
}

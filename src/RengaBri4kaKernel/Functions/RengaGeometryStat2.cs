using Renga;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Functions
{
    public class Placement3dGeomStat : PluginParametersCollection
    {
        public static Guid Placement3dAxisXId = new Guid("{8a896a86-c581-405a-b32d-eb12ab661248}");
        public const string Placement3dAxisX = "Bri4ka. Axis X";

        public static Guid Placement3dAxisYId = new Guid("{6f52f602-39ec-4e2e-a1b7-e4c359fbe625}");
        public const string Placement3dAxisY = "Bri4ka. Axis Y";

        public static Guid Placement3dAxisZId = new Guid("{eb221f96-c49c-43a2-8821-fe9c41380e0e}");
        public const string Placement3dAxisZ = "Bri4ka. Axis Z";

        public static Guid Placement3dOriginId = new Guid("{c75abd1a-04c0-464a-ae80-6b2f2190fdf0}");
        public const string Placement3dOrigin = "Bri4ka. Origin";

        public static Guid Placement3d2OriginId = new Guid("{04cd7443-6cdc-4e3b-8308-0a3958a254e2}");
        public const string Placement3d2Origin = "Bri4ka. Placement.Origin";

        public static Guid Placement3d2xAxisId = new Guid("{95f1d355-62a6-4d80-ac06-9066a4f57f65}");
        public const string Placement3d2xAxis = "Bri4ka. Placement.xAxis";

        public static Guid Placement3d2zAxisId = new Guid("{deaa1536-0151-45bc-9f7e-3f0483cb445b}");
        public const string Placement3d2zAxis = "Bri4ka. Placement.zAxis";

        public static Guid Line3D_LengthId = new Guid("{bd519da0-282d-4395-9e15-0712074adef6}");
        public const string Line3D_Length = "Bri4ka. Line3D_Length";

    }

    public class RengaGeometryStat2
    {
        public RengaGeometryStat2()
        {
            //зарегистрировать параметры, если ониотсутствуют
            RengaPropertiesUtils.RegisterPropertyIfNotReg(Placement3dGeomStat.Placement3dAxisXId, Placement3dGeomStat.Placement3dAxisX, PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(Placement3dGeomStat.Placement3dAxisYId, Placement3dGeomStat.Placement3dAxisY, PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(Placement3dGeomStat.Placement3dAxisZId, Placement3dGeomStat.Placement3dAxisZ, PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(Placement3dGeomStat.Placement3dOriginId, Placement3dGeomStat.Placement3dOrigin, PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(Placement3dGeomStat.Placement3d2OriginId, Placement3dGeomStat.Placement3d2Origin, PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(Placement3dGeomStat.Placement3d2xAxisId, Placement3dGeomStat.Placement3d2xAxis, PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(Placement3dGeomStat.Placement3d2zAxisId, Placement3dGeomStat.Placement3d2zAxis, PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(Placement3dGeomStat.Line3D_LengthId, Placement3dGeomStat.Line3D_Length, PropertyType.PropertyType_Double);

            RengaPropertiesUtils.AssignPropertiesToTypes(Placement3dGeomStat.Placement3dAxisXId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(Placement3dGeomStat.Placement3dAxisYId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(Placement3dGeomStat.Placement3dAxisZId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(Placement3dGeomStat.Placement3dOriginId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(Placement3dGeomStat.Placement3d2OriginId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(Placement3dGeomStat.Placement3d2xAxisId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(Placement3dGeomStat.Placement3d2zAxisId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(Placement3dGeomStat.Line3D_LengthId, new Guid[] {RengaObjectTypes.Line3D});
        }

        public void Calculate()
        {
            Renga.IProject? rengaProject = PluginData.Project;
            if (rengaProject == null) return;
            Renga.IModelObjectCollection rengaModelObjectCollection = rengaProject.Model.GetObjects();

            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            Guid[] props3d_ids = new Guid[] { Placement3dGeomStat.Placement3dAxisXId, Placement3dGeomStat.Placement3dAxisYId, Placement3dGeomStat.Placement3dAxisZId, Placement3dGeomStat.Placement3dOriginId, Placement3dGeomStat.Placement3d2OriginId, Placement3dGeomStat.Placement3d2xAxisId, Placement3dGeomStat.Placement3d2zAxisId };

            Guid[] props2d_ids = new Guid[] { Placement3dGeomStat.Placement3dAxisXId, Placement3dGeomStat.Placement3dAxisYId,  Placement3dGeomStat.Placement3dOriginId, Placement3dGeomStat.Placement3d2OriginId, Placement3dGeomStat.Placement3d2xAxisId };

            Guid[] line3d_params = new Guid[] { Placement3dGeomStat.Line3D_LengthId };


            for (int modelObjectIndex = 0; modelObjectIndex < rengaModelObjectCollection.Count; modelObjectIndex++)
            {
                Renga.IModelObject? rengaModelObject = rengaModelObjectCollection.GetByIndex(modelObjectIndex);
                if (rengaModelObject == null) continue;

                Renga.ILevelObject? rengaModelObjectOnLevel = null;
                try
                {
                    rengaModelObjectOnLevel = rengaModelObject.GetInterfaceByName("ILevelObject") as Renga.ILevelObject;
                }
                catch { }

                if (rengaModelObjectOnLevel != null)
                {
                    Renga.IPlacement3D pl3dInfo = rengaModelObjectOnLevel.GetPlacement();
                    rengaModelObject.SetObjectsProperties(props3d_ids, new object[]
                    {
                        $"{pl3dInfo.AxisX.X} {pl3dInfo.AxisX.Y} {pl3dInfo.AxisX.Z}",
                        $"{pl3dInfo.AxisY.X} {pl3dInfo.AxisY.Y} {pl3dInfo.AxisY.Z}",
                        $"{pl3dInfo.AxisZ.X} {pl3dInfo.AxisZ.Y} {pl3dInfo.AxisZ.Z}",
                        $"{pl3dInfo.Origin.X} {pl3dInfo.Origin.Y} {pl3dInfo.Origin.Z}",
                        $"{pl3dInfo.Placement.Origin.X} {pl3dInfo.Placement.Origin.Y} {pl3dInfo.Placement.Origin.Z}",
                        $"{pl3dInfo.Placement.xAxis.X} {pl3dInfo.Placement.xAxis.Y} {pl3dInfo.Placement.xAxis.Z}",
                        $"{pl3dInfo.Placement.zAxis.X} {pl3dInfo.Placement.zAxis.Y} {pl3dInfo.Placement.zAxis.Z}",
                    });
                }

                Renga.IPlacement2DObject? rengaModelObjectAsPlacement2DObject = null;

                try
                {
                    rengaModelObjectAsPlacement2DObject = rengaModelObject.GetInterfaceByName("IPlacement2DObject") as Renga.IPlacement2DObject;
                }
                catch { }

                if (rengaModelObjectAsPlacement2DObject != null)
                {
                    Renga.IPlacement2D pl2dInfo = rengaModelObjectAsPlacement2DObject.GetPlacement();
                    rengaModelObject.SetObjectsProperties(props2d_ids, new object[]
                    {
                        $"{pl2dInfo.AxisX.X} {pl2dInfo.AxisX.Y}",
                        $"{pl2dInfo.AxisY.X} {pl2dInfo.AxisY.Y}",
                        $"{pl2dInfo.Origin.X} {pl2dInfo.Origin.Y}",
                        $"{pl2dInfo.Placement.Origin.X} {pl2dInfo.Placement.Origin.Y}",
                        $"{pl2dInfo.Placement.xAxis.X} {pl2dInfo.Placement.xAxis.Y}"
                    });
                }

                if (rengaModelObject.ObjectType == RengaObjectTypes.Line3D)
                {
                    Renga.ILine3DParams? rengaModelObjectAsLine3D = rengaModelObject.GetInterfaceByName("ILine3DParams") as Renga.ILine3DParams;
                    if (rengaModelObjectAsLine3D != null)
                    {
                        var baseLine = rengaModelObjectAsLine3D.GetBaseline();
                        rengaModelObject.SetObjectsProperties(line3d_params, new object[]
                        {
                            baseLine.GetLength()
                        });
                    }
                }
            }
            editOperation.Apply();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Geometry;
using RengaBri4kaKernel.RengaInternalResources;
using RengaBri4kaKernel.Configs;


namespace RengaBri4kaKernel.Functions
{
    public class ParametersLinkRoomWithFloor : PluginParametersCollection
    {
        public static Guid FloorEntUniqueIdentId = new Guid("{7a63466a-6a0a-48ce-b8cd-4134d2f1b01d}");
        public const string FloorEntUniqueIdent = "Bri4ka. Идентификатор перекрытия";

        public static Guid FloorNameId = new Guid("{9ebaac8f-90cc-4801-978f-63fc45fb9a5e}");
        public const string FloorName = "Bri4ka. Наименование перекрытия";
    }

    struct TempFloorInfo
    {
        public string Name;
        public string UniqueId;
        public double Elevation;
        public Line3D LineGeometry;
    }


    public class RengaLinkRoomWithFloor
    {
        public RengaLinkRoomWithFloor()
        {
            //зарегистрировать параметры, если ониотсутствуют
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersLinkRoomWithFloor.FloorEntUniqueIdentId, ParametersLinkRoomWithFloor.FloorEntUniqueIdent, Renga.PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersLinkRoomWithFloor.FloorNameId, ParametersLinkRoomWithFloor.FloorName, Renga.PropertyType.PropertyType_String);

            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersLinkRoomWithFloor.FloorEntUniqueIdentId, new Guid[] { RengaObjectTypes.Room });
            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersLinkRoomWithFloor.FloorNameId, new Guid[] { RengaObjectTypes.Room });
        }

        public void Calculate(LinkRoomWithFloorConfig settings)
        {
            Renga.IProject? rengaProject = PluginData.Project;
            if (rengaProject == null) return;

            Renga.IModelObjectCollection? rengaAllObjects = rengaProject.Model.GetObjectsSafe();
            if (rengaAllObjects == null) return;

            //Renga.IDataExporter rengaDataExporter = rengaProject.DataExporter;
            //Renga.IExportedObject3DCollection rengaExportedObject3DCollection = rengaDataExporter.GetObjects3D();

            // Видимые Помещения
            IEnumerable<Renga.IModelObject>? rengaRoomsCollection = UserInput.GetModelObjectsByTypes(new Guid[] { RengaObjectTypes.Room }, settings.UseOnlyVisible);
            // Видимые перекрытия
            IEnumerable<Renga.IModelObject>? rengaFloorsCollection = UserInput.GetModelObjectsByTypes(new Guid[] { RengaObjectTypes.Floor }, settings.UseOnlyVisible);

            Guid[] propsIds = new Guid[] { ParametersLinkRoomWithFloor.FloorEntUniqueIdentId, ParametersLinkRoomWithFloor.FloorNameId };


            if (rengaRoomsCollection == null || !rengaRoomsCollection.Any()) return;
            if (rengaFloorsCollection == null || !rengaFloorsCollection.Any()) return;

            List<TempFloorInfo> cachedFloorsData = new List<TempFloorInfo>();

            foreach (Renga.IModelObject rengaFloorEnt in rengaFloorsCollection)
            {
                var lineGeom = rengaFloorEnt.GetLineGeometry(true);
                if (lineGeom == null) continue;

                RengaUtils.AddLog(lineGeom.ToString());

                TempFloorInfo floorInfo = new TempFloorInfo();
                
                floorInfo.LineGeometry = lineGeom;
                bool isSucessfullyCalculated;
                floorInfo.Elevation = rengaFloorEnt.GetElevation(out isSucessfullyCalculated);
                floorInfo.UniqueId = rengaFloorEnt.UniqueId.ToString("D");
                floorInfo.Name = rengaFloorEnt.Name;
                if (isSucessfullyCalculated) cachedFloorsData.Add(floorInfo);

            }

            // Анализ геометрии
            if (!cachedFloorsData.Any()) return;

            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            foreach (Renga.IModelObject rengaRoomEnt in rengaRoomsCollection)
            {
                Vector3? targetGeometry_Point = null;
                Line3D? targetGeometry_Line = null;

                bool isSucessfullyCalculated;
                double roomZ = rengaRoomEnt.GetElevation(out isSucessfullyCalculated);


                if (settings.RoomGeometryMode == RoomGeometryVariant.Centroid)
                {
                    var c = rengaRoomEnt.GetCentroid();
                    if (c == null) continue;
                    targetGeometry_Point = new Vector3(c.X, c.Y, roomZ);
                }
                else if (settings.RoomGeometryMode == RoomGeometryVariant.SolidsFloorContour)
                {
                    targetGeometry_Line = rengaRoomEnt.GetSolidsExternalContour();
                }
                else if (settings.RoomGeometryMode == RoomGeometryVariant.BaselineContour)
                {
                    targetGeometry_Line = rengaRoomEnt.GetLineGeometry(true);
                }

                // Перебираем все перекрытия
                foreach (var floorDef in cachedFloorsData)
                {
                    if (floorDef.Elevation != roomZ) continue;
                    bool isMatch = false;
                    if (settings.RoomGeometryMode == RoomGeometryVariant.Centroid) isMatch = floorDef.LineGeometry.Contains(targetGeometry_Point);
                    else
                    {
                        isMatch = floorDef.LineGeometry.Contains(targetGeometry_Line);
                    }
                    //if (
                    //    (settings.RoomGeometryMode == RoomGeometryVariant.Centroid
                    //        && floorDef.LineGeometry.Contains(targetGeometry_Point)) |
                    //    ((settings.RoomGeometryMode == RoomGeometryVariant.SolidsFloorContour | settings.RoomGeometryMode == RoomGeometryVariant.BaselineContour)
                    //         && floorDef.LineGeometry.Contains(targetGeometry_Line))
                    //    )
                    if (isMatch)
                    {
                        rengaRoomEnt.SetObjectsProperties(propsIds, new object[] { floorDef.UniqueId, floorDef.Name });
                        break;
                    }

                }
            }


            editOperation.Apply();
        }
    }
}

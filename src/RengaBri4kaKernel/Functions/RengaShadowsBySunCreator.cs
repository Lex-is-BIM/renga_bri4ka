using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renga;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Functions.SolarCalc;
using RengaBri4kaKernel.Geometry;
using RengaBri4kaKernel.RengaInternalResources;

using RengaBri4kaKernel.Configs;

namespace RengaBri4kaKernel.Functions
{

    public class ParametersShadowsBySunCreator : PLuginParametersCollection
    {
        public static Guid HourPerDayId = new Guid("{4f5e2979-c0ad-4195-8c8c-3a9ff8ee1307}");
        public const string HourPerDay = "Bri4ka. Час дня (положение солнца) ";


    }

    /// <summary>
    /// Вспомогательный класс для обработки функции "Построение теней от Солнца"
    /// </summary>
    public class RengaShadowsBySunCreator
    {
        public RengaShadowsBySunCreator()
        {
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersShadowsBySunCreator.HourPerDayId, ParametersShadowsBySunCreator.HourPerDay, PropertyType.PropertyType_String);

            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersShadowsBySunCreator.HourPerDayId, new Guid[] {RengaObjectTypes.Hatch});
        }

        public void Start(ShadowCalcParametersConfig sunParameters)
        {
            pSolarPositions = SunCalculator.GetSolarPositionsPerDay(sunParameters.Date, sunParameters.Latitude, sunParameters.Longitude, sunParameters.TimeZoneOffset);

            if (pSolarPositions == null) return;
            Guid[] propIds = new Guid[] { ParametersShadowsBySunCreator.HourPerDayId };

            //Анализируем только точки кровли (верхнюю грань) и перекрытия
            //TODO: дать пользователю выбор, на основе чего анализировать
            //Для отладки смотрим только на кровлю, потом надо включить и перекрытия
            IEnumerable<Renga.IModelObject>? analyzedObjects;
            analyzedObjects = PluginData.rengaApplication.Selection.GetSelectedObjects2(true);
            if (analyzedObjects == null) analyzedObjects = UserInput.GetModelObjectsByTypes(new Guid[] { RengaObjectTypes.Roof, RengaObjectTypes.Floor });


            if (analyzedObjects == null || !analyzedObjects.Any()) return;

            List<ShadowAnalyzedItem> analyzedPoints = new List<ShadowAnalyzedItem>();
            foreach (Renga.IModelObject rengaObject in analyzedObjects)
            {
                Renga.IExportedObject3D? rengaObjectGeometry = rengaObject.GetExportedObject3D();
                if (rengaObjectGeometry == null) continue;

                Renga.ILevelObject? objectOnLevel = rengaObject as Renga.ILevelObject;
                if (objectOnLevel == null) continue;
                

                Renga.ILevel? level = PluginData.Project.Model.GetLevel(objectOnLevel.LevelId);
                if (level == null) continue;
                double zPlus = level.Elevation / 1000.0 + objectOnLevel.ElevationAboveLevel / 1000.0 - sunParameters.GroundElevation;

                for (int rengaMeshCounter = 0; rengaMeshCounter < rengaObjectGeometry.MeshCount; rengaMeshCounter++)
                {
                    Renga.IMesh mesh = rengaObjectGeometry.GetMesh(rengaMeshCounter);

                    for (int rengaGridCounter = 0; rengaGridCounter < mesh.GridCount; rengaGridCounter++)
                    {
                        Renga.IGrid grid = mesh.GetGrid(rengaGridCounter);

                        if (rengaObject.ObjectType == RengaObjectTypes.Roof)
                        {
                            RoofGridType roofGType = (RoofGridType)grid.GridType;
                            if (roofGType == RoofGridType.Top)
                            {

                            }
                            else continue;
                        }
                        if (rengaObject.ObjectType == RengaObjectTypes.Floor && grid.GridType != (int)Renga.GridTypes.Floor.Top) continue;

                        Dictionary<int, double[]> vertices = new Dictionary<int, double[]>();

                        for (int rengaVertexCounter = 0; rengaVertexCounter < grid.VertexCount; rengaVertexCounter++)
                        {
                            Renga.FloatPoint3D p = grid.GetVertex(rengaVertexCounter);
                            // Округляем и переводим в м. (по умолчанию в Renga в мм)
                            vertices.Add(rengaVertexCounter, new double[] {
                                Math.Round(p.X / 1000.0, 3) ,
                                Math.Round(p.Y/ 1000.0, 3),
                                Math.Round(p.Z/ 1000.0, 3) });

                            analyzedPoints.Add(new ShadowAnalyzedItem(p.X, p.Y, zPlus + p.Z));
                        }
                    }
                }
            }

            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            foreach (var solarPoint in pSolarPositions)
            {
                List<ShadowResult> shadowResults = new List<ShadowResult>();
                foreach (var shadowRawInfo in analyzedPoints)
                {
                    shadowResults.Add(ShadowCalculator.CalculateColumnShadow(shadowRawInfo, solarPoint, sunParameters.GroundElevation));
                }

                //Для построенных теней нужно сформировать внешнюю границу
                var vertices = shadowResults.Select(p => p.ShadowEnd);
                //var triangles = DelaunayTriangulation.Triangulate(vertices);
                //var extContour = DelaunayTriangulation.CalculateExternalBorder(triangles);

                var extContour = DelaunayTriangulation.CalculateConvexHull(vertices);
                var bbox = BoundingBox.CalculateFromPoints(vertices);

                Renga.IModelObject? createdObject = PluginData.Project.Model.CreateBaselineObject(BaselineObjectType.Hatch, extContour, false);
                if (createdObject != null)
                {
                    PluginData.Project.Model.CreateText(bbox.GetCentroid(), $"Расчетный час: " + solarPoint.Hour.ToString());

                    //createdObject.SetObjectsProperties(propIds, new object[] {  });
                }

               
            }

            editOperation.Apply();
        }

        private List<SolarPosition>? pSolarPositions;
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.Functions
{
    internal class RengaCollisionManager
    {
        public RengaCollisionManager()
        {
            pConfig = new ClashDetectiveConfig();
        }

        public void Start(ClashDetectiveConfig? config)
        {
            if (config == null) return;
            pConfig = config;
            // Выбрать объекты первой группы
            IEnumerable<Renga.IModelObject>? group1 = UserInput.GetModelObjectsByTypes(pConfig.Group1);
            // Выбрать объекты второй группы
            IEnumerable<Renga.IModelObject>? group2 = UserInput.GetModelObjectsByTypes(pConfig.Group2);

            var oTypes = RengaUtils.GetRengaObjectTypes();

            List<SolidRelationship> needRelationsRaw = new List<SolidRelationship>();
            if (pConfig.ClashSettings.Separate) needRelationsRaw.Add(SolidRelationship.Separate);
            if (pConfig.ClashSettings.Touching) needRelationsRaw.Add(SolidRelationship.Touching);
            if (pConfig.ClashSettings.Intersecting) needRelationsRaw.Add(SolidRelationship.Intersecting);
            if (pConfig.ClashSettings.Contains) needRelationsRaw.Add(SolidRelationship.Contains);
            if (pConfig.ClashSettings.ContainedBy) needRelationsRaw.Add(SolidRelationship.ContainedBy);
            if (pConfig.ClashSettings.Equal) needRelationsRaw.Add(SolidRelationship.Equal);
            SolidRelationship[] needRelations = needRelationsRaw.ToArray();

            Dictionary<int, BRepContainsLineChecker.BRepSolid[]> objectsGeometryConverted = new Dictionary<int, BRepContainsLineChecker.BRepSolid[]>();

            if (group1 == null || group2 == null) return;

            ClashDetectiveReport report = new ClashDetectiveReport();
            foreach(Renga.IModelObject object1 in group1)
            {
                Renga.IExportedObject3D? object1Geometry = object1.GetExportedObject3D();
               
                if (object1Geometry == null) continue;
                if (!objectsGeometryConverted.ContainsKey(object1.Id)) objectsGeometryConverted.Add(object1.Id, object1Geometry.ToFacetedBRep2());

                foreach (Renga.IModelObject object2 in group2)
                {
                    Renga.IExportedObject3D? object2Geometry = object2.GetExportedObject3D();
                    if (object2Geometry == null) continue;

                    ClashDetectiveReportItem clashInfo = new ClashDetectiveReportItem()
                    {
                        NameObject1 = object1.Name,
                        ObjectId1 = object1.UniqueId,
                        CategoryObject1 = oTypes.Where(t => t.Id == object1.ObjectType).First().Name,
                        NameObject2 = object2.Name,
                        ObjectId2 = object2.UniqueId,
                        CategoryObject2 = oTypes.Where(t => t.Id == object2.ObjectType).First().Name,
                    };

                    if (!objectsGeometryConverted.ContainsKey(object2.Id)) objectsGeometryConverted.Add(object2.Id, object2Geometry.ToFacetedBRep2());

                    // проверка
                    bool isAtLeastOne = false;
                    SolidRelationship relResult = SolidRelationship.Separate;
                    foreach (var object1GeometryPart in objectsGeometryConverted[object1.Id])
                    {
                        if (isAtLeastOne) break;
                        foreach (var object2GeometryPart in objectsGeometryConverted[object2.Id])
                        {
                            SolidRelationship rel = BRepContainsLineChecker.CheckSolidRelationship(object1GeometryPart, object2GeometryPart);
                            if (needRelations.Contains(rel))
                            {
                                relResult = rel;
                                isAtLeastOne = true;
                                break;
                            }
                        }
                    }
                    if (isAtLeastOne)
                    {
                        clashInfo.Relation = relResult;
                    }

                    if (clashInfo.Relation != SolidRelationship.Separate) report.Items.Add(clashInfo);
                }
            }

            // Сохранить отчет.
            ConfigIO.SaveTo<ClashDetectiveReport>(Path.Combine(PluginConfig.GetDirectoryPath(), "CollisionReports", $"Report_{Guid.NewGuid().ToString("N")}.xml"), report);
        }

        private ClashDetectiveConfig pConfig;
    }

    
}

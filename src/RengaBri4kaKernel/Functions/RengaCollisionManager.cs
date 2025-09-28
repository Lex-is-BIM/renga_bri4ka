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
using Renga;

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
            TimerUtils.CreateInstance().Start();

            Renga.IOperation editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            pConfig = config;
            // Выбрать объекты первой группы
            IEnumerable<Renga.IModelObject>? group1 = UserInput.GetModelObjectsByTypes(pConfig.Group1);
            // Выбрать объекты второй группы
            IEnumerable<Renga.IModelObject>? group2 = UserInput.GetModelObjectsByTypes(pConfig.Group2);

            if (group1 == null || group2 == null) return;
            var oTypes = RengaUtils.GetRengaObjectTypes();

            List<SolidRelationship> needRelationsRaw = new List<SolidRelationship>();
            //if (pConfig.ClashSettings.Separate) needRelationsRaw.Add(SolidRelationship.Separate);
            if (pConfig.ClashSettings.Touching) needRelationsRaw.Add(SolidRelationship.Touching);
            if (pConfig.ClashSettings.Intersecting) needRelationsRaw.Add(SolidRelationship.Intersecting);
            if (pConfig.ClashSettings.Contains) needRelationsRaw.Add(SolidRelationship.Contains);
            if (pConfig.ClashSettings.ContainedBy) needRelationsRaw.Add(SolidRelationship.ContainedBy);
            if (pConfig.ClashSettings.Equal) needRelationsRaw.Add(SolidRelationship.Equal);
            SolidRelationship[] needRelations = needRelationsRaw.ToArray();

            Dictionary<int, IGeometryInstance[]> objectsGeometryConverted = new Dictionary<int, IGeometryInstance[]>();

            // Если среди выбранных к анализу объектов имеются помещения и текущая редакция Renga == Professional, то сделаем частичный экспорт в IFC для получения геометрий
            // TODO: реализовать, если надо ...

            

            void Set_objectsGeometryConverted(IEnumerable<Renga.IModelObject> objects)
            {
                foreach (Renga.IModelObject oneObject in objects)
                {
                    if (!objectsGeometryConverted.ContainsKey(oneObject.Id))
                    {
                        IGeometryInstance[]? geom = null;
                        Renga.IExportedObject3D? object1Geometry = oneObject.GetExportedObject3D();
                        Line3D? objectAsLine = oneObject.GetLineGeometry(config.Segmentation ?? ClashDetectiveConfig.SegmentationDefault);
                        if ((config.AnalyzeBaseLinesOnly ?? false) && objectAsLine != null) geom = new IGeometryInstance[] { objectAsLine };
                        else if (object1Geometry != null) geom = object1Geometry.ToFacetedBRep();

                        if (geom != null) objectsGeometryConverted.Add(oneObject.Id, geom);
                    }
                }
            }

            Set_objectsGeometryConverted(group1);
            Set_objectsGeometryConverted(group2);

            ClashDetectiveReport report = new ClashDetectiveReport();
            report.Settings = config;

            foreach (Renga.IModelObject object1 in group1)
            {
                foreach (Renga.IModelObject object2 in group2)
                {
                    ClashDetectiveReportItem clashInfo = new ClashDetectiveReportItem()
                    {
                        NameObject1 = object1.Name,
                        ObjectId1 = object1.UniqueId,
                        CategoryObject1 = oTypes.Where(t => t.Id == object1.ObjectType).First().Name,
                        NameObject2 = object2.Name,
                        ObjectId2 = object2.UniqueId,
                        CategoryObject2 = oTypes.Where(t => t.Id == object2.ObjectType).First().Name,
                    };

                    

                    // проверка
                    bool isAtLeastOne = false;
                    SolidRelationship relResult = SolidRelationship.Separate;
                    foreach (var object1GeometryPart in objectsGeometryConverted[object1.Id])
                    {
                        if (isAtLeastOne) break;
                        //Могут проверяться только солиды с прочей геометрией, не наоборот
                        if (object1GeometryPart.GetGeometryType() != GeometryMode.FacetedBRepSolid) continue;
                        FacetedBRepSolid? object1GeometryPart_Solid = object1GeometryPart as FacetedBRepSolid;
                        if (object1GeometryPart_Solid == null) continue;

                        foreach (var object2GeometryPart in objectsGeometryConverted[object2.Id])
                        {
                            SolidRelationship rel = SolidRelationship._Error;
                            if (object2GeometryPart.GetGeometryType() == GeometryMode.FacetedBRepSolid)
                            {
                                rel = FacetedBRepSolidChecker.CheckSolidRelationship(object1GeometryPart_Solid, object2GeometryPart as FacetedBRepSolid, config.Tolerance ?? ClashDetectiveConfig.ToleranceDefault);
                            }
                            else if (object2GeometryPart.GetGeometryType() == GeometryMode.Curve3d)
                            {
                                rel = FacetedBRepSolidChecker.ContainsLine(object1GeometryPart_Solid, object2GeometryPart as Line3D, config.Tolerance ?? ClashDetectiveConfig.ToleranceDefault);
                            }

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
                        var totalBBox = BoundingBox.GetBBoxFrom(objectsGeometryConverted[object1.Id].Select(geom => geom.GetBBox()).Concat(
                                objectsGeometryConverted[object2.Id].Select(mesh => mesh.GetBBox())));
                        clashInfo.BBoxMin = totalBBox.GetMinPoint();
                        clashInfo.BBoxMax = totalBBox.GetMaxPoint();
                    }

                    if (clashInfo.Relation != SolidRelationship.Separate)
                    {
                        report.Items.Add(clashInfo);
                        object2.CopyPropertiesFromOtherObjects(object1, config.PropertiesToCopy);
                    }
                }
            }
            editOperation.Apply();
            // Сохранить отчет.
            ConfigIO.SaveTo<ClashDetectiveReport>(Path.Combine(ClashDetectiveReport.GetSavePath(), Path.Combine($"Report_{Guid.NewGuid().ToString("N")}.xml")), report);

            
            TimerUtils.CreateInstance().Stop();


        }

        private ClashDetectiveConfig pConfig;
    }

    
}

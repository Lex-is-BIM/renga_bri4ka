using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.AuxFunctions
{
    /// <summary>
    /// Вспомогательный класс для извлечения геометрии из проекта и её кэширования
    /// </summary>
    internal class RengaProjectGeometryExtractor
    {
        public List<FacetedGeometryObjectConfig> ObjectsData { get; private set; }
        public RengaProjectGeometryExtractor()
        {
            ObjectsData = new List<FacetedGeometryObjectConfig>();
            // Инициализировать ресурсы для данного проекта
            string projectsCasheDirectory = Path.Combine(PluginConfig.GetDirectoryPath(), "ProjectsCache");
            if (!Directory.Exists(projectsCasheDirectory)) Directory.CreateDirectory(projectsCasheDirectory);

            Renga.IProject project = PluginData.Project;
            ProjectCasheDir = Path.Combine(projectsCasheDirectory, project.ProjectInfo.UniqueId.ToString("N").ToUpper());
            if (!Directory.Exists(ProjectCasheDir)) 
            {
                Directory.CreateDirectory(ProjectCasheDir);
                pCachedFiles = new List<string>();
            }
            else
            {
                pCachedFiles = Directory.GetFiles(ProjectCasheDir, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                /*
                foreach (string objectDefPath in pCachedFiles)
                {
                    FacetedGeometryObjectConfig? config = (FacetedGeometryObjectConfig?)ConfigIO.LoadFrom<FacetedGeometryObjectConfig>(objectDefPath);
                    if (config == null) continue;
                    ObjectsData.Add(config);
                }
                */
            }
            

            // init quantity types
            List<Guid> ids = new List<Guid>();
            foreach (var rengaQuantitiInfo in typeof(Renga.Quantities).GetRuntimeFields())
            {
                Guid? guid = (Guid?)rengaQuantitiInfo.GetValue(null);
                if (guid != null) ids.Add(guid.Value);
            }
            pRengaQuantityIds = ids.ToArray();
        }

        public static void DeleteCache()
        {
            Renga.IProject project = PluginData.Project;
            string projectsCasheDirectory = Path.Combine(PluginConfig.GetDirectoryPath(), "ProjectsCache");
            string ProjectCasheDir = Path.Combine(projectsCasheDirectory, project.ProjectInfo.UniqueId.ToString("N").ToUpper());
            if (!Directory.Exists(ProjectCasheDir)) Directory.Delete(ProjectCasheDir, true);
        }

        public FacetedBRepSolid[]? GetGeometryForObject(Renga.IModelObject rengaObject)
        {
            foreach (var o in this.ObjectsData)
            {
                if (o.ObjectId.Equals(rengaObject.UniqueId)) return o.Geometry.ToArray();
            }

            var needPaths = pCachedFiles.Where(path => path.Contains(rengaObject.UniqueIdS));
            if (needPaths.Any())
            {
                FacetedGeometryObjectConfig? config = (FacetedGeometryObjectConfig?)ConfigIO.LoadFrom<FacetedGeometryObjectConfig>(needPaths.First());
                if (config != null) 
                {
                    ObjectsData.Add(config);
                    return config.Geometry.ToArray();
                }
            }
            else
            {
                Renga.IExportedObject3D? geom = rengaObject.GetExportedObject3D();
                if (geom == null) return null;

                FacetedGeometryObjectConfig configNew = new FacetedGeometryObjectConfig()
                {
                    Matrix4x4 = new double[16],
                    ObjectCategoryType = rengaObject.ObjectType,
                    Parameters = new List<ObjectParameter>(),
                    Geometry = geom.ToFacetedBRep0().ToList(),
                    ObjectId = rengaObject.UniqueId
                };
                string savePath = Path.Combine(ProjectCasheDir, rengaObject.UniqueIdS + ".xml");
                this.ObjectsData.Add(configNew);
                this.pCachedFiles.Add(savePath);
                ConfigIO.SaveTo(savePath, configNew);
                return configNew.Geometry.ToArray();
            }
            return null;
        }
        public FacetedBRepSolid[]? GetGeometryForObject2(Renga.IModelObject rengaObject)
        {
            Guid objectType = rengaObject.ObjectType;
            // Получить у объекта Quantities

            List<ObjectParameter> parametersQ = new List<ObjectParameter>();
            Renga.IQuantityContainer rengaQuantitiesCollection = rengaObject.GetQuantities();

            foreach (Guid rengaQuantityId in pRengaQuantityIds)
            {
                if (rengaQuantitiesCollection.Contains(rengaQuantityId))
                {
                    Renga.IQuantity rengaQuantity = rengaQuantitiesCollection.Get(rengaQuantityId);
                    double? propValue = RengaPropertiesUtils.GetQuantityValue(rengaQuantity);
                    if (propValue == null) continue;
                    parametersQ.Add(new ObjectParameter() { Id = rengaQuantityId, Value = Math.Round(propValue.Value, 3) });
                }
            }

            // Получить информацию о матрице
            Renga.ILevelObject? rengaObjectOnLevel = rengaObject as Renga.ILevelObject;
            if (rengaObjectOnLevel == null) return null;

            Renga.ITransform3D tr3d = rengaObjectOnLevel.GetPlacement().GetTransformInto();
            FacetedGeometryObjectConfig? existedConfig = GetForCondition(objectType, parametersQ);
            //existedConfig = null;
            if (existedConfig == null)
            {
                // Нужно добавить

                Renga.IExportedObject3D? geom = rengaObject.GetExportedObject3D();
                if (geom == null) return null;
                var geomConverted = geom.ToFacetedBRep0();
                if (geomConverted == null) return null;
                
                FacetedGeometryObjectConfig configNew = new FacetedGeometryObjectConfig()
                {
                    Matrix4x4 = tr3d.GetTransformMatrix(),
                    ObjectCategoryType = objectType,
                    Parameters = parametersQ,
                    Geometry = geomConverted.ToList(),
                    ObjectId = rengaObject.UniqueId
                };
                this.ObjectsData.Add(configNew);
                ConfigIO.SaveTo(Path.Combine(ProjectCasheDir, rengaObject.UniqueIdS + ".xml"), configNew);
                return geomConverted;
            }
            else
            {
                // Нужно пересчитать имеющуюся геометрию
                // Создаем формулу для старой матрицы (для которой есть геометрия)
                var tr3dTmp = tr3d.GetCopy();
                tr3dTmp.SetTransformMatrix(existedConfig.Matrix4x4);

                // Calculate the relative transformation from first to second object
                double[] worldToSecond = RengaBri4kaKernel.Geometry.Matrix4x4.Inverse(tr3d.GetTransformMatrix());

                // Combined transformation: first object local space -> world space -> second object local space
                double[] firstToSecond = RengaBri4kaKernel.Geometry.Matrix4x4.Multiply(worldToSecond, existedConfig.Matrix4x4);

                List<FacetedBRepSolid> solidList = new List<FacetedBRepSolid>();

                foreach (var solid in existedConfig.Geometry)
                {
                    FacetedBRepSolid solidNew = new FacetedBRepSolid();
                    
                    
                    foreach (var face in solid.Faces)
                    {
                        Face f = new Face();
                        foreach (var vertInfo in face.Vertices)
                        {
                            f.GetOrAddVertexIndex(TransformPoint3d(firstToSecond, vertInfo));
                        }
                        solidNew.AddFace(f);
                    }
                    solidList.Add(solidNew);
                }

                return solidList.ToArray();
            }
        }

        private Geometry.Vector3 TransformPoint3d(double[] Matrix, Geometry.Vector3 xyz)
        {
            double x = xyz.X * Matrix[0 * 4 + 0] + xyz.Y * Matrix[0 * 4 + 1] + xyz.Z * Matrix[0 * 4 + 2] + Matrix[0 * 4 + 3];
            double y = xyz.X * Matrix[1 * 4 + 0] + xyz.Y * Matrix[1 * 4 + 1] + xyz.Z * Matrix[1 * 4 + 2] + Matrix[1 * 4 + 3];
            double z = xyz.X * Matrix[2 * 4 + 0] + xyz.Y * Matrix[2 * 4 + 1] + xyz.Z * Matrix[2 * 4 + 2] + Matrix[2 * 4 + 3];
            double w = xyz.X * Matrix[3 * 4 + 0] + xyz.Y * Matrix[3 * 4 + 1] + xyz.Z * Matrix[3 * 4 + 2] + Matrix[3 * 4 + 3];

            //Perspective division if w != 1
            if (w != 1 && w != 0)
            {
                x /= w;
                y /= w;
                z /= w;
            }

            return new Geometry.Vector3(x, y, z);
        }

        private FacetedGeometryObjectConfig? GetForCondition(Guid objectType, List<ObjectParameter> parameters)
        {
            for (int i = 0; i < this.ObjectsData.Count; i++)
            {
                FacetedGeometryObjectConfig? c = this.ObjectsData[i];
                if (!c.ObjectCategoryType.Equals(objectType)) continue;
                if (c.Parameters.Count != parameters.Count) continue;

                bool isAtLeastOneValueOther = false;
                for (int propIndex = 0; propIndex < parameters.Count; propIndex++)
                {
                    for (int propIndex2 = 0; propIndex2 < parameters.Count; propIndex2++)
                    {
                        if (c.Parameters[propIndex].Id.Equals(parameters[propIndex2].Id))
                        {
                            if (c.Parameters[propIndex].Value != parameters[propIndex2].Value)
                            {
                                isAtLeastOneValueOther = true;
                                break;
                            }
                        }
                    }
                    if (isAtLeastOneValueOther)
                    {
                        break;
                    }
                }
                if (isAtLeastOneValueOther) continue;

                return c;
            }

            return null;
        }

        private List<string> pCachedFiles; 
        private Guid[] pRengaQuantityIds;
        private string ProjectCasheDir;
    }
}

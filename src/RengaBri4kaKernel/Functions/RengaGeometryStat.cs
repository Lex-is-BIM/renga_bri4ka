using Renga;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Functions
{
    public class ParametersGeometryStat : PLuginParametersCollection
    {
        public static Guid MeshesCountId = new Guid("{b9a92ee2-5706-439f-ace6-67943297fb3b}");
        public const string MeshesCount = "Bri4ka. Число мэшей";

        public static Guid GridsCountId = new Guid("{c879721e-fe77-4980-906c-f8e642e27893}");
        public const string GridsCount = "Bri4ka. Число сеток";

        public static Guid VerticesCountId = new Guid("{ce316e68-a00e-4fe2-8c99-e39b9a025f3e}");
        public const string VerticesCount = "Bri4ka. Число вершин";

        public static Guid TrianglesCountId = new Guid("{09b657f1-0fd1-4580-86d2-96789bf3bc1f}");
        public const string TrianglesCount = "Bri4ka. Число граней";

        public static Guid NormalsCountId = new Guid("{711a555b-636b-48d9-b32b-4f7260737abf}");
        public const string NormalsCount = "Bri4ka. Число нормалей";

    }

    public class RengaGeometryStat
    {

        public RengaGeometryStat()
        {
            //зарегистрировать параметры, если ониотсутствуют
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersGeometryStat.MeshesCountId, ParametersGeometryStat.MeshesCount, PropertyType.PropertyType_Integer);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersGeometryStat.GridsCountId, ParametersGeometryStat.GridsCount, PropertyType.PropertyType_Integer);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersGeometryStat.VerticesCountId, ParametersGeometryStat.VerticesCount, PropertyType.PropertyType_Integer);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersGeometryStat.TrianglesCountId, ParametersGeometryStat.TrianglesCount, PropertyType.PropertyType_Integer);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersGeometryStat.NormalsCountId, ParametersGeometryStat.NormalsCount, PropertyType.PropertyType_Integer);

            //TODO: создавать только для объектов, имеющих 3D-представление
            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersGeometryStat.MeshesCountId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersGeometryStat.GridsCountId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersGeometryStat.VerticesCountId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersGeometryStat.TrianglesCountId, null);
            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersGeometryStat.NormalsCountId, null);
        }

        public void Calculate()
        {
            Renga.IProject? rengaProject = PluginData.Project;
            if (rengaProject == null) return;
            Renga.IDataExporter rengaDataExporter = rengaProject.DataExporter;
            Renga.IExportedObject3DCollection rengaExportedObject3DCollection = rengaDataExporter.GetObjects3D();
            Renga.IModelObjectCollection rengaModelObjectCollection = rengaProject.Model.GetObjects();

            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            Guid[] propIds = new Guid[] { ParametersGeometryStat.MeshesCountId, ParametersGeometryStat.GridsCountId, ParametersGeometryStat.VerticesCountId, ParametersGeometryStat.TrianglesCountId, ParametersGeometryStat.NormalsCountId };
            for (int rengaObjectGeometryCounter = 0; rengaObjectGeometryCounter < rengaExportedObject3DCollection.Count; rengaObjectGeometryCounter++)
            {
                Renga.IExportedObject3D rengaObjectGeometry = rengaExportedObject3DCollection.Get(rengaObjectGeometryCounter);

                int meshesCount = rengaObjectGeometry.MeshCount;
                int gridsCount = 0;
                int verticesCount = 0;
                int trianglesCount = 0;
                int normalsCount = 0;

                rengaObjectGeometry.GetGeometryStatistics(out meshesCount, out gridsCount, out verticesCount, out trianglesCount, out normalsCount);
                rengaModelObjectCollection.GetById(rengaObjectGeometry.ModelObjectId).SetObjectsProperties(propIds, new object[] { meshesCount, gridsCount, verticesCount, trianglesCount, normalsCount });
            }

            editOperation.Apply();
        }

    }
}

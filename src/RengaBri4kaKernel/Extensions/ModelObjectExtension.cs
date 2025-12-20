using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RengaBri4kaKernel.Extensions
{
    internal static class ModelObjectExtension
    {
        public static void CopyPropertiesFromOtherObjects(this Renga.IModelObject rengaObject, Renga.IModelObject otherObject, Guid[]? propIds)
        {
            if (PluginData.Project == null) return;
            if (propIds == null) return;
            Renga.IPropertyManager propsManagerProject = PluginData.Project.PropertyManager;

            // Добавляем в объект определения свойств
            //Renga.IOperation editOperation = PluginData.Project.CreateOperation();
            //editOperation.Start();

            List<Guid> idCollection = new List<Guid>();
            List<object?> dataCollection = new List<object?>();

            //var propsManagerThis = rengaObject.GetProperties();
            var propsManager = otherObject.GetProperties();
            Renga.IGuidCollection props = propsManager.GetIds();
            for (int propCounter = 0; propCounter < props.Count; propCounter++)
            {
                Guid propId = props.Get(propCounter);
                if (propIds.Contains(propId))
                {
                    if (!propsManagerProject.IsPropertyAssignedToType(propId, rengaObject.ObjectType))
                    {
                        propsManagerProject.AssignPropertyToType(propId, rengaObject.ObjectType);
                    }
                    idCollection.Add(propId);
                    dataCollection.Add(propsManager.Get(propId).GetPropertyValue());
                }
            }

            if (idCollection.Any()) rengaObject.SetObjectsProperties(idCollection.ToArray(), dataCollection.ToArray());

            //editOperation.Apply();
        }

        public static void SetObjectsProperties(this Renga.IModelObject rengaObject, Guid[]? propsId, object?[]? propsData)
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
                object? propData = propsData[propCounter];
                var propDef = PluginData.Project.PropertyManager.GetPropertyDescription(propId);

                if (propsManager.Contains(propId) && propData != null)
                {
                    Renga.IProperty? propInfo = propsManager.Get(propId);
                    propInfo.SetPropertyValue(propData);
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

        public static Line3D? GetExternalBorder(this Renga.IModelObject rengaObject, int? gridType)
        {
            Renga.IExportedObject3D? geom = rengaObject.GetExportedObject3D();
            if (geom == null) return null;

            List<Point3D> points = new List<Point3D>();

            for (int rengaMeshCounter = 0; rengaMeshCounter < geom.MeshCount; rengaMeshCounter++)
            {
                Renga.IMesh mesh = geom.GetMesh(rengaMeshCounter);

                for (int rengaGridCounter = 0; rengaGridCounter < mesh.GridCount; rengaGridCounter++)
                {
                    Renga.IGrid grid = mesh.GetGrid(rengaGridCounter);
                    if (gridType != null && (grid.GridType != gridType.Value)) continue;
                    for (int rengaVertexCounter = 0; rengaVertexCounter < grid.VertexCount; rengaVertexCounter++)
                    {
                        Renga.FloatPoint3D p = grid.GetVertex(rengaVertexCounter);
                        points.Add(new Point3D(p.X, p.Y, p.Z));

                    }
                }
            }
            //ConcaveHull.Compute(points, 3) ;

            //var triangles = DelaunayTriangulation.Triangulate(points);
            //var extContour = DelaunayTriangulation.CalculateExternalBorder(triangles); // DelaunayTriangulation.CalculateConvexHull(points);
            return new Line3D(points);
        }

        public static Line3D? GetLineGeometry(this Renga.IModelObject rengaObject, int segmentation)
        {
            object rengaObjectAsBaseline2DObjectRaw = rengaObject.GetInterfaceByName("IBaseline2DObject");
            if (rengaObjectAsBaseline2DObjectRaw != null)
            {
                Renga.IBaseline2DObject? rengaObjectAsBaseline2DObject = rengaObjectAsBaseline2DObjectRaw as Renga.IBaseline2DObject;
                if (rengaObjectAsBaseline2DObject != null)
                {
                    Renga.ICurve2D curve2d = rengaObjectAsBaseline2DObject.GetBaselineInCS(
                        new Renga.Placement2D()
                        {
                            Origin = new Renga.Point2D() { X = 0, Y = 0 },
                            xAxis = new Renga.Vector2D() { X = 1, Y = 0 }
                        });
                    return RengaGeometryConverter.FromCurve2d_2(curve2d);
                }
            }
            
            
            if (rengaObject.ObjectType.Equals(Renga.EntityTypes.Beam))
            {
                Renga.IBeamParams? objAsBeam = rengaObject as Renga.IBeamParams;
                if (objAsBeam != null) return RengaGeometryConverter.FromCurve3d_2(objAsBeam.GetBaseline());
            }
            if (rengaObject.ObjectType.Equals(Renga.EntityTypes.Column))
            {
                Renga.IColumnParams? objAsColumn = rengaObject as Renga.IColumnParams;
                if (objAsColumn != null) return new Line3D()
                {
                    Vertices = new List<Vector3>()
                    {
                        new Vector3(objAsColumn.Position.X, objAsColumn.Position.Y, objAsColumn.Position.Z),
                        new Vector3(objAsColumn.Position.X, objAsColumn.Position.Y, objAsColumn.Position.Z + objAsColumn.Height),

                    }
                };
            }
            //if (rengaObject.ObjectType.Equals(Renga.EntityTypes.Floor))
            //{
            //    Renga.IFloorParams? objAsFloor = rengaObject as Renga.IFloorParams;
            //    if (objAsFloor != null) return RengaGeometryConverter.FromCurve2d(objAsFloor.GetContour());
            //}
            if (rengaObject.ObjectType.Equals(Renga.EntityTypes.Route))
            {
                Renga.IRouteParams? objAsRoute = rengaObject as Renga.IRouteParams;
                if (objAsRoute != null) return RengaGeometryConverter.FromCurve3d(objAsRoute.GetContour(), segmentation);
            }

            return null;
        }
    }
}

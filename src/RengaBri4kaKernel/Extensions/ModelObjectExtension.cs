using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Geometry;
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

        public static Line3D? GetLineGeometry(this Renga.IModelObject rengaObject, int segmentation)
        {
            Renga.IBaseline2DObject? rengaObjectAsBaseline2DObject = rengaObject as Renga.IBaseline2DObject;
            if (rengaObjectAsBaseline2DObject != null)
            {
                Renga.ICurve2D curve2d = rengaObjectAsBaseline2DObject.GetBaseline();
                return RengaGeometryConverter.FromCurve2d(curve2d, segmentation);
            }
            
            if (rengaObject.ObjectType.Equals(Renga.ObjectTypes.Beam))
            {
                Renga.IBeamParams? objAsBeam = rengaObject as Renga.IBeamParams;
                if (objAsBeam != null) return RengaGeometryConverter.FromCurve3d(objAsBeam.GetBaseline(), segmentation);
            }
            if (rengaObject.ObjectType.Equals(Renga.ObjectTypes.Column))
            {
                Renga.IColumnParams? objAsColumn = rengaObject as Renga.IColumnParams;
                if (objAsColumn != null) return new Line3D()
                {
                    Points = new List<Point3D>()
                    {
                        new Point3D(objAsColumn.Position.X, objAsColumn.Position.Y, objAsColumn.Position.Z),
                        new Point3D(objAsColumn.Position.X, objAsColumn.Position.Y, objAsColumn.Position.Z + objAsColumn.Height),

                    }
                };
            }
            //if (rengaObject.ObjectType.Equals(Renga.ObjectTypes.Floor))
            //{
            //    Renga.IFloorParams? objAsFloor = rengaObject as Renga.IFloorParams;
            //    if (objAsFloor != null) return RengaGeometryConverter.FromCurve2d(objAsFloor.GetContour());
            //}
            if (rengaObject.ObjectType.Equals(Renga.ObjectTypes.Route))
            {
                Renga.IRouteParams? objAsRoute = rengaObject as Renga.IRouteParams;
                if (objAsRoute != null) return RengaGeometryConverter.FromCurve3d(objAsRoute.GetContour(), segmentation);
            }

            return null;
        }
    }
}

using Microsoft.Win32;
using RengaBri4kaGis;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Functions
{
    public enum ImportType
    {
        VectorData,
        RasterData
    }
    public class RengaGeodataImporter
    {
        private RengaGeodataImporter()
        {
            // Загрузить ГИС
            string executingAssemblyFile = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath;
            string rootDirPath = new DirectoryInfo(Path.GetDirectoryName(executingAssemblyFile)).Parent.FullName;


            string osgeoLibPath = Path.Combine(rootDirPath, "osgeo");
            try
            {
                var gisAss = Assembly.LoadFrom(Path.Combine(osgeoLibPath, "RengaBri4kaGis.dll"));
                if (gisAss != null) isGisLoaded = true;

                RengaBri4kaGis.GisLoader.Initialize(osgeoLibPath);
            }
            catch (Exception ex) { }
        }

        public static RengaGeodataImporter CreateInstance()
        {
            if (mInstance == null) mInstance = new RengaGeodataImporter();
            return mInstance;
        }

        public void Start(ImportType mode)
        {
            if (!this.isGisLoaded)
            {
                RengaUtils.ShowMessageBox("RengaBri4kaGis не был успешно загружен!. Выполнение функции невозможно");
                return;
            }

            pImportedMode = mode;

            

#if DEBUG
            pInputFiles = new string[] { @"C:\Users\Georg\Documents\GitHub\TBS-GIS\samples\СПБ_МСК-1964\6718d993-2606-4c3d-b916-84ba702c20f4.osm" };
#endif
            //SelectFiles();

            mProjectTransformParameters = PluginData.Project.SiteInfo.GetTransformPararameters();

            if (pImportedMode == ImportType.VectorData)
            {
                OgrProcessing();
            }

        }


        private void SelectFiles()
        {
            GisProvidersCollection driversCollection;
            if (pImportedMode == ImportType.VectorData) driversCollection = GisProvidersCollection.GetOgrGrivers();
            else driversCollection = GisProvidersCollection.GetGdalGrivers();

            OpenFileDialog open_files = new OpenFileDialog();
            open_files.Multiselect = true;
            open_files.Title = "Выборка файлов для импорта";

            List<string> filters = new List<string>();
            List<string> exts = new List<string>();

            foreach (var driver_info in driversCollection.Providers)
            {
                //Делаем поисковой паттерн, добавляя * к имени файла и делая расширения также заглавными
                var current_exts = driver_info.Extensions.Concat(driver_info.Extensions.Select(a => a = a.ToUpper()));
                string extensions = string.Join(";", current_exts);
                exts = exts.Concat(current_exts).ToList();
                filters.Add($"{driver_info.Caption} ({extensions}) | {extensions} ");
            }
            exts = exts.Distinct().ToList();
            filters.Add($"_Все файлы | {string.Join(";", exts)}");

            filters.Sort();
            filters = filters.Distinct().ToList();

            open_files.Filter = string.Join("|", filters);

            if (open_files.ShowDialog() == true)
            {
                pInputFiles = open_files.FileNames;
            }
        }

        private void OgrProcessing()
        {
            if (pInputFiles == null || !pInputFiles.Any()) return;

            Renga.IModel rengaModel = PluginData.Project.Model;
            Dictionary<Guid, List<string>> registeredProps = new Dictionary<Guid, List<string>>();
            Dictionary<string, Guid> registeredPropsIdMap = new Dictionary<string, Guid>();

            foreach (string inputFilePath in pInputFiles)
            {
                OgrDataSource ogrFile = new OgrDataSource(inputFilePath);
                if (ogrFile.IsNull()) continue;

                for (int layerIndex = 0; layerIndex < ogrFile.GetLayersCount(); layerIndex++)
                {
                    OgrLayer? layer = ogrFile.GetLayer(layerIndex);
                    if (layer == null) continue;

                    OsrSpatialReference? layerCS = layer.GetSpatialRef();

                    OsrReprojectTool osrReproject = new OsrReprojectTool(layerCS, new OsrSpatialReference(mProjectTransformParameters.ProjectedCoordinateSystem));

                    List<OgrFeature>? features = layer.GetFeatures();
                    if (features == null) continue;

                    foreach (OgrFeature? feature in features)
                    {
                        if (feature == null) continue;
                        OgrGeometry? featureGeometry = feature.GetGeometry();
                        if (featureGeometry == null) continue;
                        osrReproject.Reproject(ref featureGeometry);

                        GeometryType featureGeometryType = featureGeometry.GetGeometryType();

                        
                        //Ogr_SimpleGeometry? convertedGeometryDef = featureGeometry.ToSimpleGeometry();
                        //if (convertedGeometryDef == null) continue;

                        //List<Renga.IModelObject?> createdObjects0 = new List<Renga.IModelObject>();

                        List<Renga.IModelObject?>? getRengaObjects(Ogr_SimpleGeometry? convertedGeometryDef)
                        {
                            if (convertedGeometryDef == null) return null;

                            List<Renga.IModelObject?> createdObjects = new List<Renga.IModelObject?>();

                            switch (featureGeometryType)
                            {
                                case GeometryType.Point:
                                    {
                                        importPoint((Ogr_Point)convertedGeometryDef);
                                        break;
                                    }
                                case GeometryType.MultiPoint:
                                    {
                                        Ogr_MultiPoint? ogr_MPoint = (Ogr_MultiPoint)convertedGeometryDef;
                                        if (ogr_MPoint == null) break;
                                        foreach (var ogrPoint in ogr_MPoint.Points)
                                        {
                                            importPoint(ogrPoint);
                                        }
                                        break;
                                    }
                                case GeometryType.LineString:
                                    {
                                        importPolyline((Ogr_LineString)convertedGeometryDef);
                                        break;
                                    }
                                case GeometryType.MultiLineString:
                                    {
                                        Ogr_MultiLineString? ogr_MLineString = (Ogr_MultiLineString)convertedGeometryDef;
                                        if (ogr_MLineString == null) break;
                                        foreach (var ogrLineString in ogr_MLineString.LineStrings)
                                        {
                                            importPolyline(ogrLineString);
                                        }
                                        break;
                                    }
                                case GeometryType.Polygon:
                                    {
                                        importPolygon((Ogr_Polygon)convertedGeometryDef);
                                        break;
                                    }
                                case GeometryType.MyltiPolygon:
                                    {
                                        Ogr_MultiPolygon? ogr_MPolygon = (Ogr_MultiPolygon)convertedGeometryDef;
                                        if (ogr_MPolygon == null) break;
                                        foreach (var ogrPolygon in ogr_MPolygon.Polygons)
                                        {
                                            importPolygon(ogrPolygon);
                                        }
                                        break;
                                    }
                                case GeometryType.GeometryCollection:
                                    {
                                        Ogr_GeometryCollection? ogr_GColl = (Ogr_GeometryCollection)convertedGeometryDef;
                                        if (ogr_GColl == null) break;
                                        foreach (var ogrSomeGeometry in ogr_GColl.Geometries)
                                        {
                                            List<Renga.IModelObject?>? objectsCollection = getRengaObjects(ogrSomeGeometry);
                                            if (objectsCollection != null)
                                            {
                                                foreach (var rengaObject in objectsCollection)
                                                {
                                                    if (rengaObject != null) createdObjects.Add(rengaObject);
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }

                            void importPoint(Ogr_Point? ogrPoint)
                            {
                                if (ogrPoint != null)
                                {
                                    createdObjects.Add(rengaModel.CreateBaselineObject(BaselineObjectType.Line3dAsCircle, new List<Geometry.Point3D>() { new Geometry.Point3D(ogrPoint.X / 1000.0, ogrPoint.Y / 1000.0, ogrPoint.Z / 1000.0) }));
                                }
                            }

                            void importPolyline(Ogr_LineString? ogrLineString)
                            {
                                if (ogrLineString != null)
                                {
                                    for (int vertexIndex = 0; vertexIndex < ogrLineString.Vertices.Length - 1; vertexIndex++)
                                    {
                                        Ogr_Point v1 = ogrLineString.Vertices[vertexIndex];
                                        Ogr_Point v2 = ogrLineString.Vertices[vertexIndex + 1];

                                        Geometry.Point3D p1 = new Geometry.Point3D(v1.X / 1000.0, v1.Y / 1000.0, v1.Z / 1000.0);
                                        Geometry.Point3D p2 = new Geometry.Point3D(v2.X / 1000.0, v2.Y / 1000.0, v2.Z / 1000.0);

                                        createdObjects.Add(rengaModel.CreateBaselineObject(BaselineObjectType.Line3d, new List<Geometry.Point3D>() { p1, p2 }));
                                    }
                                }
                            }

                            void importPolylineAsHatch(Ogr_LineString? ogrLineString)
                            {
                                if (ogrLineString != null)
                                {
                                    List<Geometry.Point3D> points = new List<Geometry.Point3D>();
                                    for (int vertexIndex = 0; vertexIndex < ogrLineString.Vertices.Length; vertexIndex++)
                                    {
                                        Ogr_Point v1 = ogrLineString.Vertices[vertexIndex];
                                        points.Add(new Geometry.Point3D(v1.X / 1000.0, v1.Y / 1000.0, v1.Z / 1000.0));
                                    }
                                    createdObjects.Add(rengaModel.CreateBaselineObject(BaselineObjectType.Hatch, points));
                                }
                            }

                            void importPolygon(Ogr_Polygon? ogrPolygon)
                            {
                                if (ogrPolygon != null)
                                {
                                    importPolylineAsHatch(ogrPolygon.ExternalContour);
                                }
                            }

                            return createdObjects;
                        }

                        List<Renga.IModelObject?>? createdObjects0 = getRengaObjects(featureGeometry.ToSimpleGeometry());
                        if (createdObjects0 == null || !createdObjects0.Any()) continue;
                        // Для объектов нужно назначить атрибуты (бррр)
                        Dictionary<string, string> featureProperties = feature.Properties;

                        foreach (Renga.IModelObject? importedObject in createdObjects0)
                        {
                            if (importedObject == null) continue;
                            Guid oType = importedObject.ObjectType;
                            if (!registeredProps.ContainsKey(oType)) registeredProps[oType] = new List<string>();

                            Guid[] propsIds = new Guid[featureProperties.Count];
                            object[] propsData = new object[featureProperties.Count];
                            for (int propIndex = 0;  propIndex < propsIds.Length; propIndex++)
                            {
                                var propData = featureProperties.ElementAt(propIndex);
                                string propKey = propData.Key;
                                Guid propId;
                                if (!registeredProps[oType].Contains(propKey))
                                {
                                    propId = Guid.NewGuid();
                                    RengaPropertiesUtils.RegisterPropertyIfNotReg(propId, propKey, Renga.PropertyType.PropertyType_String);
                                    RengaPropertiesUtils.AssignPropertiesToTypes(propId, new Guid[] { oType });

                                    registeredPropsIdMap[propKey] = propId;
                                }
                                else propId = registeredPropsIdMap[propKey];

                                propsIds[propIndex] = propId;
                                propsData[propIndex] = propData.Value;
                            }

                            importedObject.SetObjectsProperties(propsIds, propsData);
                        }
                    }
                }
            }

            
        }

        private ImportType pImportedMode;
        private string[]? pInputFiles;
        private ProjectTransformPararameters mProjectTransformParameters;

        private bool isGisLoaded = false;
        private static RengaGeodataImporter? mInstance = null;

    }
}

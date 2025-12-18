using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;
using Microsoft.Win32;

using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.Functions
{
    public class RengaElevationImporterSettings
    {
        public double IsolinesStep = 0.5;
        public bool UseCoordsOffset = true;

        public string? SelectedSurfaceName;
        public bool AddExternalBorder = true;
    }

    public class LandXML_SurfaceDef
    {
        public Dictionary<int, Point3D> Pnts { get; set; }
        public int[][] Faces { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public string Name { get; set; }

        public LandXML_SurfaceDef()
        {
            Pnts = new Dictionary<int, Point3D>();
            Metadata = new Dictionary<string, string> { };
            Faces = new int[][] { };
            Name = "";
        }
    }

    internal class RengaElevationImporter
    {
        public RengaElevationImporter()
        {
            this.pRengaBuildingTransformInfo = PluginData.Project.BuildingInfo.GetTransformPararameters();
            this.pAngleSinus = Math.Sin(this.pRengaBuildingTransformInfo[3]);
            this.pAngleCos = Math.Cos(this.pRengaBuildingTransformInfo[3]);
        }

        /// <summary>
        /// Инициирует импорт LandXML-файла и возвращает имена поверхностей из файла
        /// </summary>
        /// <returns></returns>
        public string[]? SetLandXML(string? filePath)
        {
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Title = "Выбор LandXML файла";
            //openFileDialog.Multiselect = false;
            //openFileDialog.Filter = "LandXML файл (*.XML, *.xml) | *.XML;*.xml";

            //if (openFileDialog.ShowDialog() == true && File.Exists(openFileDialog.FileName))
            //{
            //    mLandXML = XDocument.Load(openFileDialog.FileName);
            //}

            if (filePath == null || !File.Exists(filePath)) return null;
            mLandXML = XDocument.Load(filePath);

            if (mLandXML == null) return null;
            List<string> surfNames = new List<string>();
            IEnumerable<XElement> surfaces = mLandXML.Descendants().Where(a => a.Name.LocalName == "Surface");
            foreach (XElement surface in surfaces)
            {
                surfNames.Add(surface.Attribute("name")?.Value ?? "");
            }
            surfNames = surfNames.Distinct().ToList();

            IsXml = true;
            return surfNames.ToArray();
        }

        private void ReCalcPoints(ref double x, ref double y, ref double z)
        {
            x -= pRengaBuildingTransformInfo[0];
            y -= pRengaBuildingTransformInfo[1];
            z -= pRengaBuildingTransformInfo[2];

            x = x * pAngleCos - y * pAngleSinus;
            y = x * pAngleSinus + y * pAngleCos;
        }


        public void Import(RengaElevationImporterSettings settings)
        {
            if (mLandXML == null) return;
            LandXML_SurfaceDef? surfaceDef = new LandXML_SurfaceDef();
            DelaunayTriangulation.Triangle[]? triangles2 = null;
            IEnumerable<XElement> surfaces = mLandXML.Descendants().Where(a => a.Name.LocalName == "Surface");
            foreach (XElement surface in surfaces)
            {
                string surfName = surface.Attribute("name").Value;

                if (settings.SelectedSurfaceName != surfName) continue;

                //Dictionary<int, Renga.FloatPoint3D> surface_points = new Dictionary<int, FloatPoint3D>();
                //List<int[]> surface_triangles = new List<int[]>();
                XElement points = surface.Descendants().Where(a => a.Name.LocalName == "Pnts").First();
                XElement triangles = surface.Descendants().Where(a => a.Name.LocalName == "Faces").First();

                //surfaceDef.Faces = new int[triangles.Elements().Count()][];
                triangles2 = new DelaunayTriangulation.Triangle[triangles.Elements().Count()];

                foreach (XElement point in points.Elements())
                {
                    int point_id = Convert.ToInt32(point.Attribute("id").Value);
                    double[] point_coords = point.Value.Split(' ').Select(a => Convert.ToDouble(a)).ToArray();
                    if (settings.UseCoordsOffset) ReCalcPoints(ref point_coords[1], ref point_coords[0], ref point_coords[2]);
                    Point3D pnt = new Point3D(point_coords[1], point_coords[0], point_coords[2]);
                    surfaceDef.Pnts.Add(point_id, pnt);
                }
                int triangleCounter = 0;
                foreach (XElement triangle in triangles.Elements())
                {
                    int[] points_indexes = triangle.Value.Split(' ').Select(a => Convert.ToInt32(a)).ToArray();
                    //surfaceDef.Faces[triangleCounter] = points_indexes;
                    //triangleCounter++;

                    // Преобразование граней в иное представление
                    DelaunayTriangulation.Triangle trg =
                        new DelaunayTriangulation.Triangle(
                            surfaceDef.Pnts[points_indexes[0]], surfaceDef.Pnts[points_indexes[1]], surfaceDef.Pnts[points_indexes[2]]);
                    triangles2[triangleCounter] = trg;
                    triangleCounter++;

                }
            }

            var allZ = surfaceDef.Pnts.Values.Select(p => p.Z).ToList();

            // Если выбрана опция генерации доп. контура, то нужно пересчитать триангуляцию с учетом новых точек контура
            if (settings.AddExternalBorder)
            {
                var existedSurfaceVertices = surfaceDef.Pnts.Values.Cast<Point3D>().ToList();

                var allX0 = existedSurfaceVertices.Select(p => p.X);
                var allY0 = existedSurfaceVertices.Select(p => p.Y);
                var allZ0 = existedSurfaceVertices.Select(p => p.Z);

                double xMin0 = allX0.Min() - 10.0;
                double xMax0 = allX0.Max() + 10.0;

                double yMin0 = allY0.Min() - 10.0;
                double yMax0 = allY0.Max() + 10.0;

                double minZ0 = allZ0.Min() - 1.0;
                allZ.Add(minZ0);

                existedSurfaceVertices.Add(new Point3D(xMin0, yMin0, minZ0));
                existedSurfaceVertices.Add(new Point3D(xMin0, yMax0, minZ0));
                existedSurfaceVertices.Add(new Point3D(xMax0, yMax0, minZ0));
                existedSurfaceVertices.Add(new Point3D(xMax0, yMin0, minZ0));

                triangles2 = DelaunayTriangulation.Triangulate(existedSurfaceVertices).ToArray();
            }

            // ФОрмируем Z, для которых нужно вычислить горизонтали
            List<double> isolineLevels = new List<double>();
            
            double minZ = allZ.Min();
            double maxZ = allZ.Max();

            double minZ2 = Math.Floor(minZ);
            int levelSteps = Convert.ToInt32(Math.Ceiling((maxZ - minZ2) / settings.IsolinesStep));
            for (int levelStep = 0; levelStep < levelSteps; levelStep++)
            {
                isolineLevels.Add(minZ2 + levelStep * settings.IsolinesStep);
            }

            // Generate isolines
            IsolineGenerator generatorIso = new IsolineGenerator();
            var isolines = generatorIso.GenerateIsolines(triangles2, isolineLevels);
            var polylines = generatorIso.ConnectSegmentsIntoPolylines(isolines);

            Debug.WriteLine($"Generated {isolines.Count} contour segments");
            Debug.WriteLine($"Connected into {polylines.Count} polylines");

            Renga.IModel? rModel = PluginData.GetModel();
            if (rModel != null)
            {
                foreach (var polyline in polylines)
                {
                    var polyline2 = DelaunayTriangulation.CalculateConvexHull(polyline);

                    Renga.IModelObject? rengaObject = rModel.CreateBaselineObject(BaselineObjectType.Floor, polyline2, true);
                    if (rengaObject == null) continue;

                    Renga.IOperation editOperation = PluginData.Project.CreateOperation();
                    editOperation.Start();

                    var floorThicknessParam = rengaObject.GetParameters().Get(Renga.ParameterIds.FloorThickness);
                    floorThicknessParam.SetDoubleValue(settings.IsolinesStep * 1000.0);

                    editOperation.Apply();
                }
            }
            
        }

        private double pAngleSinus;
        private double pAngleCos;

        private LandXML_SurfaceDef[]? mLandXML_Surfaces;
        public bool IsXml = false;
        private XDocument? mLandXML;
        private double[] pRengaBuildingTransformInfo;
    }
}

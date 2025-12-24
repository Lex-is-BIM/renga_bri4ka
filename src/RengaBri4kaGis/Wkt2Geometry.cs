using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RengaBri4kaGis
{
    internal class Wkt2Geometry
    {
        public Wkt2Geometry(string wkt, GeometryType gType)
        {
            pOgrWkt = wkt;
            pGeometryType = gType;
        }


        public Ogr_Point? AsPoint()
        {
            if (this.pGeometryType == GeometryType.Point)
            {
                double[] p = get_one_point(get_sub_geometry(this.pOgrWkt));
                return new Ogr_Point(p[0], p[1], p[2]);
            }
            else return null;
        }
        public Ogr_LineString? AsPline()
        {
            if (this.pGeometryType == GeometryType.LineString)
            {
                double[][] points = get_some_points(get_sub_geometry(this.pOgrWkt));
                Ogr_LineString lineString = new Ogr_LineString(points.Length);
                for(int i = 0; i < points.Length; i++)
                {
                    double[] p = points[i];
                    lineString.Vertices[i] = new Ogr_Point(p[0], p[1], p[2]);
                }
                return lineString;
            }
            else return null;
        }
        public Ogr_Polygon? AsPolygon()
        {
            if (this.pGeometryType == GeometryType.Polygon)
            {
                double[][][] plgRaw = get_some_plines(get_sub_geometry(this.pOgrWkt));
                Ogr_LineString? pldExternalContour = null;
                Ogr_LineString[]? pldInternalContours = null;

                if (plgRaw.Length > 1) pldInternalContours = new Ogr_LineString[plgRaw.Length - 1];

                for (int plgContourIndex = 0; plgContourIndex < plgRaw.Length; plgContourIndex++)
                {
                    double[][] points = plgRaw[plgContourIndex];
                    Ogr_LineString lineString = new Ogr_LineString(points.Length);
                    for (int i = 0; i < points.Length; i++)
                    {
                        double[] p = points[i];
                        lineString.Vertices[i] = new Ogr_Point(p[0], p[1], p[2]);
                    }
                    if (plgContourIndex == 0) pldExternalContour = lineString;
                    else pldInternalContours[plgContourIndex - 1] = lineString;
                }

                if (pldExternalContour == null) return null;

                Ogr_Polygon plg = new Ogr_Polygon(pldExternalContour);
                plg.AddInternalContours(pldInternalContours);


                return plg;
            }
            else return null;
        }
        //multy
        //public double[][] AsMPoints()
        //{
        //    if (this.pGeometryType == GeometryType.MultiPoint)
        //    {
        //        return get_some_points(get_sub_geometry(this.pOgrWkt));
        //    }
        //    else return null;
        //}
        //public double[][][] AsMPlines()
        //{
        //    if (this.pGeometryType == GeometryType.MultiLineString)
        //    {
        //        return get_some_plines(get_sub_geometry(this.pOgrWkt));
        //    }
        //    else return null;
        //}

        /*(...,...,...)*/
        private string get_sub_geometry(string full_geometry)
        {
            int t1 = full_geometry.IndexOf("(") + 1;
            int t2 = full_geometry.LastIndexOf(")");
            if (t1 == 0) t1 = 0;
            if (t2 == -1) t2 = full_geometry.Length;

            return full_geometry.Substring(t1, t2 - t1);
        }
        /*(),(),()...*/
        List<string> get_sub_geometries(string full_geometry)
        {
            List<string> parts = new List<string>();
            string one_part = "";
            bool br_1 = false;
            bool br_2 = false;
            
            foreach (char ch in full_geometry) 
			{
                if (ch == '(') br_1 = true;
                if (ch == ')') br_2 = true;
                one_part += ch.ToString();

                if (br_1 && br_2)
                {
                    parts.Add(get_sub_geometry(one_part));
                    one_part = "";
                    br_1 = false;
                    br_2 = false;
                }
            }
            if (br_1 && br_2)
            {
                parts.Add(get_sub_geometry(one_part));
                one_part = "";
                br_1 = false;
                br_2 = false;
            }
            return parts;
        }

        double[] get_one_point(string str)
        {
            var p_arr_str = str.Split(' ');
            double[] p_arr = new double[p_arr_str.Length];

            int i = 0;
            foreach(var p_arr_part in p_arr_str)
            {
                p_arr[i] = Convert.ToDouble(p_arr_part);
                i++;
            }
            if (p_arr.Length == 3)
            {
                p_arr = new double[] { p_arr[0], p_arr[1], p_arr[2] };
            }
            else
            {
                p_arr = new double[] { p_arr[0], p_arr[1], 0.0};
            }
            return p_arr;
        }

        double[][] get_some_points(string str)
        {
            var points_arr = get_sub_geometry(str).Split(',');
            double[][]  ps = new double[points_arr.Length][];

            int i = 0;
            foreach(var one_point in points_arr)
            {
                ps[i] = get_one_point(one_point);
                i++;
            }
            return ps;
        }
        double[][][] get_some_plines(string str)
        {
            var geoms = get_sub_geometries(str);
            int i = 0;
            double[][][] plines = new double[geoms.Count][][];
            foreach(var part in geoms)
            {
                plines[i] = get_some_points(part);
                i++;
            }
            return plines;
        }

        

        private GeometryType pGeometryType;
        private string pOgrWkt;
    }
}

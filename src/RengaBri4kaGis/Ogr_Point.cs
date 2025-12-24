using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public class Ogr_Point : Ogr_SimpleGeometry
    {
        public Ogr_Point(double x, double y, double z = 0.0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public GeometryType GetGeometryType()
        {
            return GeometryType.Point;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}

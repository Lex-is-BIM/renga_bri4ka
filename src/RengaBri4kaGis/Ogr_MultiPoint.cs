using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public class Ogr_MultiPoint : Ogr_SimpleGeometry
    {
        public Ogr_MultiPoint(int n)
        {
            Points = new Ogr_Point[n];
        }

        public Ogr_Point[] Points { get; set; }

        public GeometryType GetGeometryType()
        {
            return GeometryType.MultiPoint;
        }
    }
}

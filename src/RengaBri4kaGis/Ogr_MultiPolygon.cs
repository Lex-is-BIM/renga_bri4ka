using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public class Ogr_MultiPolygon : Ogr_SimpleGeometry
    {
        public Ogr_MultiPolygon(int n)
        {
            Polygons = new Ogr_Polygon[n];
        }

        public Ogr_Polygon[] Polygons { get; set; }

        public GeometryType GetGeometryType()
        {
            return GeometryType.MyltiPolygon;
        }
    }
}

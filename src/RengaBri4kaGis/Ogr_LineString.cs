using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public class Ogr_LineString : Ogr_SimpleGeometry
    {
        public Ogr_LineString(int vertLength)
        {
            Vertices = new Ogr_Point[vertLength];
        }

        public Ogr_Point[] Vertices { get; set; }

        public GeometryType GetGeometryType()
        {
            return GeometryType.LineString;
        }
    }
}

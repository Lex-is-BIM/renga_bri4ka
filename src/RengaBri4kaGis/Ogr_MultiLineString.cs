using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public class Ogr_MultiLineString : Ogr_SimpleGeometry
    {
        public Ogr_MultiLineString(int n)
        {
            LineStrings = new Ogr_LineString[n];
        }

        public Ogr_LineString[] LineStrings { get; set; }

        public GeometryType GetGeometryType()
        {
            return GeometryType.MultiLineString;
        }
    }
}

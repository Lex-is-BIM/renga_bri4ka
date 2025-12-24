using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public class Ogr_Polygon : Ogr_SimpleGeometry
    {
        public Ogr_Polygon(Ogr_LineString extContour)
        {
            ExternalContour = extContour;
        }

        public void AddInternalContours(Ogr_LineString[] contours)
        {
            InternalContours = contours;
        }

        public Ogr_LineString ExternalContour { get; set; }
        public Ogr_LineString[]? InternalContours { get; set; }

        public GeometryType GetGeometryType()
        {
            return GeometryType.Polygon;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public class Ogr_GeometryCollection : Ogr_SimpleGeometry
    {
        public Ogr_GeometryCollection(int n)
        {
            Geometries = new Ogr_SimpleGeometry[n];
        }

        public Ogr_SimpleGeometry[] Geometries { get; set; }

        public GeometryType GetGeometryType()
        {
            return GeometryType.GeometryCollection;
        }
    }
}

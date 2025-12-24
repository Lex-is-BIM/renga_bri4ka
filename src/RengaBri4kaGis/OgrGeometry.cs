using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public enum GeometryType : int
    {
        Point,
        MultiPoint,
        LineString,
        MultiLineString,
        Polygon,
        MyltiPolygon,
        GeometryCollection,
        TIN_Surface,
        Other
    };


    public class OgrGeometry
    {
        private static GeometryType GetByNativeType(wkbGeometryType _OGRwkbGeometryType)
        {
            GeometryType geometryType = GeometryType.Other;
            switch (_OGRwkbGeometryType)
            {
                default:
                    geometryType = GeometryType.Other;
                    break;
                case wkbGeometryType.wkbPoint25D:
                case wkbGeometryType.wkbPoint:
                case wkbGeometryType.wkbPointZM:
                case wkbGeometryType.wkbPointM:
                    geometryType = GeometryType.Point;
                    break;
                case wkbGeometryType.wkbMultiPoint:
                case wkbGeometryType.wkbMultiPointM:
                case wkbGeometryType.wkbMultiPointZM:
                case wkbGeometryType.wkbMultiPoint25D:
                    geometryType = GeometryType.MultiPoint;
                    break;
                case wkbGeometryType.wkbLineString:
                case wkbGeometryType.wkbLineStringZM:
                case wkbGeometryType.wkbLineStringM:
                case wkbGeometryType.wkbLineString25D:
                    geometryType = GeometryType.LineString;
                    break;
                case wkbGeometryType.wkbMultiLineString:
                case wkbGeometryType.wkbMultiLineStringZM:
                case wkbGeometryType.wkbMultiLineStringM:
                case wkbGeometryType.wkbMultiLineString25D:
                    geometryType = GeometryType.MultiLineString;
                    break;
                case wkbGeometryType.wkbPolygon:
                case wkbGeometryType.wkbPolygonZM:
                case wkbGeometryType.wkbPolygonM:
                case wkbGeometryType.wkbPolygon25D:
                case wkbGeometryType.wkbTriangle:
                case wkbGeometryType.wkbTriangleZM:
                case wkbGeometryType.wkbTriangleM:
                case wkbGeometryType.wkbTriangleZ:
                    geometryType = GeometryType.Polygon;
                    break;
                case wkbGeometryType.wkbMultiPolygon:
                case wkbGeometryType.wkbMultiPolygonZM:
                case wkbGeometryType.wkbMultiPolygonM:
                case wkbGeometryType.wkbMultiPolygon25D:
                    geometryType = GeometryType.MyltiPolygon;
                    break;
                case wkbGeometryType.wkbGeometryCollection:
                case wkbGeometryType.wkbGeometryCollectionZM:
                case wkbGeometryType.wkbGeometryCollectionM:
                case wkbGeometryType.wkbGeometryCollection25D:
                    geometryType = GeometryType.GeometryCollection;
                    break;
                case wkbGeometryType.wkbPolyhedralSurface:
                case wkbGeometryType.wkbPolyhedralSurfaceZM:
                case wkbGeometryType.wkbPolyhedralSurfaceM:
                case wkbGeometryType.wkbPolyhedralSurfaceZ:
                case wkbGeometryType.wkbTIN:
                case wkbGeometryType.wkbTINZM:
                case wkbGeometryType.wkbTINM:
                case wkbGeometryType.wkbTINZ:
                    geometryType = GeometryType.TIN_Surface;
                    break;
            }
            return geometryType;
        }



        internal OgrGeometry(Geometry geometry)
        {
            mGeometry = geometry;
        }

        public GeometryType GetGeometryType()
        {
            return GetByNativeType(mGeometry.GetGeometryType());
        }

        public string? ToWkt()
        {
            string? wkt = null;
            if (mGeometry != null)
            {
                int status = mGeometry.ExportToWkt(out wkt);
            }

            return wkt;
        }

        public Ogr_SimpleGeometry? ToSimpleGeometry()
        {
            string? wkt = ToWkt();
            if (wkt == null) return null;

            
            var geomertyType = GetGeometryType();
            switch (geomertyType)
            {
                case GeometryType.Point:
                case GeometryType.LineString:
                case GeometryType.Polygon:
                    {
                        Wkt2Geometry helperConverter = new Wkt2Geometry(wkt, GetGeometryType());
                        if (geomertyType == GeometryType.Point) return helperConverter.AsPoint();
                        else if (geomertyType == GeometryType.LineString) return helperConverter.AsPline();
                        else if (geomertyType == GeometryType.Polygon) return helperConverter.AsPolygon();
                        return null;
                    }
                case GeometryType.MultiPoint:
                case GeometryType.MultiLineString:
                case GeometryType.MyltiPolygon:
                case GeometryType.GeometryCollection:
                    {
                        int subGeometriesCount = this.mGeometry.GetGeometryCount();
                        Ogr_SimpleGeometry targetGeometry;
                        if (geomertyType == GeometryType.MultiPoint) targetGeometry = new Ogr_MultiPoint(subGeometriesCount);
                        else if (geomertyType == GeometryType.MultiLineString) targetGeometry = new Ogr_MultiLineString(subGeometriesCount);
                        else if (geomertyType == GeometryType.MyltiPolygon) targetGeometry = new Ogr_MultiPolygon(subGeometriesCount);
                        else targetGeometry = new Ogr_GeometryCollection(subGeometriesCount);

                        for (int gIndex = 0; gIndex < subGeometriesCount; gIndex++)
                        {
                            Geometry subGeometryNative = this.mGeometry.GetGeometryRef(gIndex);
                            if (subGeometryNative == null) continue;
                            OgrGeometry subGeometryWrapped = new OgrGeometry(subGeometryNative);
                            Ogr_SimpleGeometry? subGeometryConverted = subGeometryWrapped.ToSimpleGeometry();
                            if (subGeometryConverted == null) continue;

                            if (geomertyType == GeometryType.MultiPoint && subGeometryConverted.GetGeometryType() == GeometryType.Point) ((Ogr_MultiPoint)targetGeometry).Points[gIndex] = (Ogr_Point)subGeometryConverted;
                            else if (geomertyType == GeometryType.MultiLineString && subGeometryConverted.GetGeometryType() == GeometryType.LineString) ((Ogr_MultiLineString)targetGeometry).LineStrings[gIndex] = (Ogr_LineString)subGeometryConverted;
                            else if (geomertyType == GeometryType.MyltiPolygon && subGeometryConverted.GetGeometryType() == GeometryType.Polygon) ((Ogr_MultiPolygon)targetGeometry).Polygons[gIndex] = (Ogr_Polygon)subGeometryConverted;
                            else ((Ogr_GeometryCollection)targetGeometry).Geometries[gIndex] = subGeometryConverted;


                        }
                        return targetGeometry;
                    }
                default:
                    return null;
            }
        }

        internal Geometry mGeometry;
    }
}

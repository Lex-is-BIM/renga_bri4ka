using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public class ProviderInfo
    {
        public string Name { get; set; }

        public string Caption { get; set; }
        public string[] Extensions { get; set; }

        public ProviderInfo(string name, string caption, string[] extensions)
        {
            this.Name = name;
            this.Caption = caption;
            this.Extensions = extensions;
        }
    }
    public class GisProvidersCollection
    {
        public static GisProvidersCollection GetOgrGrivers()
        {
            GisProvidersCollection collection = new GisProvidersCollection();
            collection.Providers = new ProviderInfo[]
            {
                new ProviderInfo("DGN", "Microstation DGN", new string[]{"*.dgn"}),
                new ProviderInfo("DXF", "AutoCAD DXF", new string[]{"*.dxf"}),
                new ProviderInfo("GeoJSON", "GeoJSON", new string[]{"*.json", "*.geojson"}),
                new ProviderInfo("GML", "Geography Markup Language", new string[]{"*.gml"}),
                new ProviderInfo("GPKG", "GeoPackage vector", new string[]{"*.gpkg"}),
                new ProviderInfo("GPX", "GPS Exchange Format", new string[]{"*.gpx"}),
                new ProviderInfo("KML", "Keyhole Markup Language", new string[]{"*.kml"}),
                new ProviderInfo("LIBKML", "LIBKML Driver", new string[]{"*.kml", "*.kmz"}),
                new ProviderInfo("MapInfo File", "MapInfo TAB and MIF/MID", new string[]{"*.mid", "*.mif", "*.tab"}),
                new ProviderInfo("OSM", "OpenStreetMap XML and PBF", new string[]{"*.osm"}),
                new ProviderInfo("ESRI Shapefile", "ESRI Shapefile / DBF", new string[]{"*.shp"})
            };
            return collection;
        }

        public static GisProvidersCollection GetGdalGrivers()
        {
            GisProvidersCollection collection = new GisProvidersCollection();
            collection.Providers = new ProviderInfo[]
            {

            };
            return collection;
        }

        public ProviderInfo[] Providers { get; internal set; }
    }
}

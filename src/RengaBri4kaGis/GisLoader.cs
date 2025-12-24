using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;

using OSGeo.GDAL;
using OSGeo.OGR;



namespace RengaBri4kaGis
{
    public class GisLoader
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetEnvironmentVariable(string lpName, string lpValue);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        public static void Initialize(string basePath)
        {
            string GDAL_HOME = basePath;  // for example , "osgeo"

#if DEBUG
            //GDAL_HOME = @"D:\PROCESSING\SDK\OSGEO\release-1930-x64-dev\release-1930-x64\bin";
#endif
            string path = Environment.GetEnvironmentVariable("PATH");
            path += ";" + GDAL_HOME;
            SetEnvironmentVariable("PATH", path);
            SetDllDirectory(GDAL_HOME);

            string gdalDataPath = Path.Combine(GDAL_HOME, "gdal-data");
            SetEnvironmentVariable("GDAL_DATA", gdalDataPath);

            string projPath = Path.Combine(GDAL_HOME, "proj9", "share");
            SetEnvironmentVariable("PROJ_LIB", projPath);

            Ogr.RegisterAll();
            Gdal.AllRegister();

            //foreach (var provider in GisProvidersCollection.GetOgrGrivers().Providers)
            //{
            //    OSGeo.OGR.Driver driver = Ogr.GetDriverByName(provider.Name);
            //    if (driver != null) driver.Register();
            //}

            //foreach (var provider in GisProvidersCollection.GetGdalGrivers().Providers)
            //{
            //    OSGeo.GDAL.Driver driver = Gdal.GetDriverByName(provider.Name);
            //    if (driver != null) driver.Register();
            //}
        }
    }
}

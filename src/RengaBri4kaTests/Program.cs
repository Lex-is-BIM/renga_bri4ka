using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaGis;

namespace RengaBri4kaTests
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string executingAssemblyFile = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath;
            //string rootDirPath = new DirectoryInfo(Path.GetDirectoryName(executingAssemblyFile)).Parent.FullName;

            string osgeoLibPath = Path.GetDirectoryName(executingAssemblyFile);// Path.Combine(rootDirPath, "osgeo");
            try
            {
                var gisAss = Assembly.LoadFrom(Path.Combine(osgeoLibPath, "RengaBri4kaGis.dll"));

                RengaBri4kaGis.GisLoader.Initialize(osgeoLibPath);
            }
            catch (Exception ex) { }

            string inputFilePath = @"C:\Users\Georg\Documents\GitHub\TBS-GIS\samples\СПБ_МСК-1964\export_1.geojson";
            OgrDataSource ogrFile = new OgrDataSource(inputFilePath);

            return;
        }
    }
}

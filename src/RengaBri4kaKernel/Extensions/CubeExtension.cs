using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{

    internal static class CubeExtension
    {

        public static Renga.Point3D GetMinPointMeters(this Renga.Cube rengaCube)
        {
            return new Renga.Point3D() { X = rengaCube.MIN.X / 1000.0, Y = rengaCube.MIN.Y / 1000.0, Z = rengaCube.MIN.Z / 1000.0 };
        }

        public static Renga.Point3D GetMaxPointMeters(this Renga.Cube rengaCube)
        {
            return new Renga.Point3D() { X = rengaCube.MAX.X / 1000.0, Y = rengaCube.MAX.Y / 1000.0, Z = rengaCube.MAX.Z / 1000.0 };
        }

        public static double GetArea(this Renga.Cube rengaCube)
        {
            var minP = rengaCube.GetMinPointMeters();
            var maxP = rengaCube.GetMaxPointMeters();
            return (maxP.X - minP.X) * (maxP.Y - minP.Y);
        }

        public static double GetVolume(this Renga.Cube rengaCube)
        {
            var minP = rengaCube.GetMinPointMeters();
            var maxP = rengaCube.GetMaxPointMeters();

            return (maxP.X - minP.X) * (maxP.Y - minP.Y) * (maxP.Z - minP.Z);
        }

        public static string GetMinPointMetersStr(this Renga.Cube rengaCube)
        {
            return getPointInMetersStr(rengaCube.GetMinPointMeters());
        }

        public static string GetMaxPointMetersStr(this Renga.Cube rengaCube)
        {
            return getPointInMetersStr(rengaCube.GetMaxPointMeters());
        }

        private static string getPointInMetersStr(Renga.Point3D rengaPoint)
        {
            return $"{(rengaPoint.X).ToString("0.00")};{(rengaPoint.Y).ToString("0.00")};{(rengaPoint.Z).ToString("0.00")}";
        }
    }
}

using Renga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{
    internal static class Placement3DExtension
    {
        public static double[] Position = new double[] { 0, 0, 0 };
        public static void Init(this Placement3D placement, double x = 0, double y = 0, double z = 0)
        {
            placement.Origin = new Renga.Point3D() { X = x, Y = y, Z = z };
            placement.xAxis = new Vector3D() { X = 1, Y = 0, Z = 0 };
            placement.zAxis = new Vector3D() { X = 0, Y = 0, Z = 1 };

        }
    }
}

using RengaBri4kaKernel.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.AuxFunctions
{
    internal class RengaGeometryConverter
    {
        public static Line3D? FromCurve3d(Renga.ICurve3D? curve3d, int segmentation)
        {
            if (curve3d == null) return null;
            double dParam = curve3d.MaxParameter - curve3d.MinParameter;
            Line3D line = new Line3D();

            for (int i = 0; i < segmentation; i++)
            {
                var p = curve3d.GetPointOn(curve3d.MinParameter + dParam / segmentation);
                line.Points.Add(new Point3D(p.X, p.Y, p.Z));
            }
            return line;
        }

        public static Line3D? FromCurve2d(Renga.ICurve2D? curve2d, int segmentation)
        {
            if (curve2d == null) return null;
            double dParam = curve2d.MaxParameter - curve2d.MinParameter;
            Line3D line = new Line3D();

            for (int i = 0; i < segmentation; i++)
            {
                var p = curve2d.GetPointOn(curve2d.MinParameter + dParam / segmentation);
                line.Points.Add(new Point3D(p.X, p.Y, 0));
            }
            return line;
        }
    }
}

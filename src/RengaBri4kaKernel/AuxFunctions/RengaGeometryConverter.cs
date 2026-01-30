using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.AuxFunctions
{
    internal class RengaGeometryConverter
    {
        public static Line3D? FromCurve3d(Renga.ICurve3D? curve3d, int segmentation = 10)
        {
            if (curve3d == null) return null;
            double dParam = curve3d.MaxParameter - curve3d.MinParameter;
            Line3D line = new Line3D();

            for (int i = 0; i < segmentation; i++)
            {
                var p = curve3d.GetPointOn(curve3d.MinParameter + dParam * (i / segmentation));
                line.Vertices.Add(new Vector3(p.X, p.Y, p.Z));
            }
            return line;
        }

        public static Line3D? FromCurve3d_2(Renga.ICurve3D? curve3d)
        {
            if (curve3d == null) return null;
            double dParam = curve3d.MaxParameter - curve3d.MinParameter;
            Line3D line = new Line3D();

            for (int i = Convert.ToInt32(curve3d.MinParameter); i < Convert.ToInt32(curve3d.MaxParameter); i++)
            {
                double param = Convert.ToInt32(curve3d.MinParameter) + i;
                var p = curve3d.GetPointOn(param);
                line.Vertices.Add(new Vector3(p.X, p.Y, p.Z));
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
                double param = curve2d.MinParameter + dParam * (i / segmentation);
                var p = curve2d.GetPointOn(param);
                line.Vertices.Add(new Vector3(p.X, p.Y, 0));
            }
            return line;
        }

        public static Line3D? FromCurve2d_2(Renga.ICurve2D? curve2d, double elevation)
        {
            if (curve2d == null) return null;
            double dParam = curve2d.MaxParameter - curve2d.MinParameter;
            Line3D line = new Line3D();

            for (int i = Convert.ToInt32(curve2d.MinParameter); i < Convert.ToInt32(curve2d.MaxParameter); i++)
            {
                double param = Convert.ToInt32(curve2d.MinParameter) + i;
                var p = curve2d.GetPointOn(param);
                line.Vertices.Add(new Vector3(p.X, p.Y, elevation));
            }
            return line;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public class Line3D : IGeometryInstance
    {
        public List<Point3D> Points;
        public BoundingBox? BBox;
        public Line3D()
        {
            Points = new List<Point3D>();
        }

        public override GeometryMode GetGeometryType()
        {
            return GeometryMode.Curve3d;
        }

        public override BoundingBox GetBBox()
        {
            if (this.BBox == null) this.BBox = BoundingBox.CalculateFromPoints(this.Points);
            return this.BBox;
        }
    }
}

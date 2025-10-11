using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public class Line3D : IGeometryInstance
    {
        public List<Vector3> Vertices;
        public BoundingBox? BBox;
        public Line3D()
        {
            Vertices = new List<Vector3>();
        }

        public Line3D(IEnumerable<Point3D> points)
        {
            Vertices = new List<Vector3>();
            foreach (var point in points)
            {
                Vertices.Add(new Vector3(point.X, point.Y, point.Z));
            }
        }

        public override GeometryMode GetGeometryType()
        {
            return GeometryMode.Curve3d;
        }

        public override BoundingBox GetBBox()
        {
            if (this.BBox == null) this.BBox = BoundingBox.CalculateFromPoints(this.Vertices);
            return this.BBox;
        }
    }
}

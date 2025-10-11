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

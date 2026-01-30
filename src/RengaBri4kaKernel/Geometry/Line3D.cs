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
        public double Elevation;
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

        public Point3D GetCentroid()
        {
            var bbox = GetBBox();
            return new Point3D((bbox.MinX + bbox.MaxX) / 2.0, (bbox.MinY + bbox.MaxY) / 2.0, (bbox.MinZ + bbox.MaxZ) / 2.0);
        }

        public bool Contains(Line3D? otherLine)
        {
            if (otherLine == null) return false;
            foreach (var otherLineVertex in otherLine.Vertices)
            {
                if (Contains(otherLineVertex)) return false;
            }
            return false;
        }

        public bool Contains(Vector3? point)
        {
            if (point == null) return false;
            if (Vertices.Count < 3) throw new ArgumentException("Polygon must have at least 3 vertices");

            int count = Vertices.Count;
            bool inside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                // Check if point is exactly on a vertex
                if (point.Equals(Vertices[i]) || point.Equals(Vertices[j]))
                    return true; // or false depending on your boundary inclusion preference

                // Get current and next vertex
                Vector3 pi = Vertices[i];
                Vector3 pj = Vertices[j];

                // Check if ray intersects with edge (pi, pj)
                if (((pi.Y > point.Value.Y) != (pj.Y > point.Value.Y)) &&
                    (point.Value.X < (pj.X - pi.X) * (point.Value.Y - pi.Y) / (pj.Y - pi.Y) + pi.X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            int counter = 0;

            string id = Guid.NewGuid().ToString("D");
            foreach (Vector3 v in Vertices)
            {
                str.AppendLine($"{id}, {counter}," + v.ToString().Replace("(", "").Replace(")", ""));
                counter++;
            }
            return str.ToString();
        }


    }
}

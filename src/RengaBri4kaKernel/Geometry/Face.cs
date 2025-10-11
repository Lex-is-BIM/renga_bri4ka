using Renga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RengaBri4kaKernel.Geometry
{
    public class Face
    {
        public List<Vector3> Vertices { get; set; }
        public Vector3 Normal { get; set; }

        public Face()
        {
            Vertices = new List<Vector3>();
            Normal = new Vector3();
        }

        public void CalculateNormal()
        {
            Normal = CalculateNormal(Vertices);
        }

        public int GetOrAddVertexIndex(Vector3 vertex)
        {
            // Check if vertex already exists
            for (int i = 0; i < Vertices.Count; i++)
            {
                if (VectorEquals(Vertices[i], vertex))
                {
                    return i;
                }
            }

            // Add new vertex
            Vertices.Add(vertex);
            return Vertices.Count - 1;
        }

        private static bool VectorEquals(Vector3 a, Vector3 b)
        {
            return (a - b).LengthSquared() < 1e-10;
        }

        private static Vector3 CalculateNormal(List<Vector3> vertices)
        {
            if (vertices.Count < 3)
            {
#if DEBUG
                throw new ArgumentException("Face needs at least 3 vertices");
#else
                return new Vector3(0, 0, 1);
#endif
            }

            // Use Newell's method for robust normal calculation
            Vector3 normal = new Vector3(0, 0, 0);
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 next = vertices[(i + 1) % vertices.Count];

                normal.X += (current.Y - next.Y) * (current.Z + next.Z);
                normal.Y += (current.Z - next.Z) * (current.X + next.X);
                normal.Z += (current.X - next.X) * (current.Y + next.Y);
            }

            return normal.Normalized();
        }

        public bool IsPointOnFace(Vector3 point, double tolerance = 1e-10)
        {
            // Check if point is coplanar with the face
            if (Math.Abs(DistanceToPlane(point)) > tolerance)
                return false;

            // Use point-in-polygon test with ray casting
            return IsPointInPolygon(point, Vertices, Normal);
        }

        private double DistanceToPlane(Vector3 point)
        {
            // Plane equation: (point - vertices[0]) · normal = 0
            return (point - Vertices[0]).Dot(Normal);
        }

        private bool IsPointInPolygon(Vector3 point, List<Vector3> polygon, Vector3 normal)
        {
            // Project polygon and point to 2D
            var (projectedPoint, projectedPolygon) = ProjectTo2D(point, polygon, normal);

            // Ray casting algorithm
            int crossings = 0;
            int n = projectedPolygon.Count;

            for (int i = 0; i < n; i++)
            {
                Point2D a = projectedPolygon[i];
                Point2D b = projectedPolygon[(i + 1) % n];

                if (RayCrossesSegment(projectedPoint, a, b))
                    crossings++;
            }

            return crossings % 2 == 1;
        }

        private (Point2D, List<Point2D>) ProjectTo2D(Vector3 point, List<Vector3> polygon, Vector3 normal)
        {
            // Find dominant axis for projection
            Vector3 axis = new Vector3(Math.Abs(normal.X), Math.Abs(normal.Y), Math.Abs(normal.Z));
            int dropIndex = axis.X > axis.Y ? (axis.X > axis.Z ? 0 : 2) : (axis.Y > axis.Z ? 1 : 2);

            List<Point2D> projected = new List<Point2D>();
            foreach (var vertex in polygon)
            {
                projected.Add(new Point2D(
                    dropIndex == 0 ? vertex.Y : vertex.X,
                    dropIndex == 2 ? vertex.Y : vertex.Z
                ));
            }

            return (new Point2D(
                dropIndex == 0 ? point.Y : point.X,
                dropIndex == 2 ? point.Y : point.Z
            ), projected);
        }

        private bool RayCrossesSegment(Point2D point, Point2D a, Point2D b)
        {
            // Check if ray from point to right crosses segment ab
            if ((a.Y > point.Y) == (b.Y > point.Y))
                return false;

            if (b.Y == a.Y)
                return false;

            double xIntersect = a.X + (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y);
            return xIntersect > point.X;
        }
    }
}

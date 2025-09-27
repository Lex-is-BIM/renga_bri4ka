using Renga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public class Face
    {
        public List<int> Vertices { get; set; }
        public Point3D Normal { get; private set; }
        public readonly FacetedBRepSolid Owner;

        public Face(FacetedBRepSolid owner)
        {
            Owner = owner;
            Vertices = new List<int>();
            Normal = new Point3D();
        }

        public void CalculateNormal()
        {
            Normal = CalculateNormal(Owner.GetPoints(Vertices));
        }

        private static Point3D CalculateNormal(List<Point3D> vertices)
        {
            if (vertices.Count < 3)
                throw new ArgumentException("Face needs at least 3 vertices");

            // Use Newell's method for robust normal calculation
            Point3D normal = new Point3D(0, 0, 0);
            for (int i = 0; i < vertices.Count; i++)
            {
                Point3D current = vertices[i];
                Point3D next = vertices[(i + 1) % vertices.Count];

                normal.X += (current.Y - next.Y) * (current.Z + next.Z);
                normal.Y += (current.Z - next.Z) * (current.X + next.X);
                normal.Z += (current.X - next.X) * (current.Y + next.Y);
            }

            return normal.Normalize();
        }

        public bool IsPointOnFace(Point3D point, double tolerance = 1e-10)
        {
            // Check if point is coplanar with the face
            if (Math.Abs(DistanceToPlane(point)) > tolerance)
                return false;

            // Use point-in-polygon test with ray casting
            return IsPointInPolygon(point, Owner.GetPoints(Vertices), Normal);
        }

        private double DistanceToPlane(Point3D point)
        {
            // Plane equation: (point - vertices[0]) · normal = 0
            return (point - Owner.Points[Vertices[0]]).Dot(Normal);
        }

        private bool IsPointInPolygon(Point3D point, List<Point3D> polygon, Point3D normal)
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

        private (Point2D, List<Point2D>) ProjectTo2D(Point3D point, List<Point3D> polygon, Point3D normal)
        {
            // Find dominant axis for projection
            Point3D axis = new Point3D(Math.Abs(normal.X), Math.Abs(normal.Y), Math.Abs(normal.Z));
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

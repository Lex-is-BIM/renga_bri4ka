using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public class BRepContainsLineChecker
    {
        // Basic geometric structures
        public struct Point3D
        {
            public double X, Y, Z;
            public Point3D(double x, double y, double z) { X = x; Y = y; Z = z; }

            public static Point3D operator -(Point3D a, Point3D b) => new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            public static Point3D operator +(Point3D a, Point3D b) => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            public static Point3D operator *(Point3D a, double scalar) => new Point3D(a.X * scalar, a.Y * scalar, a.Z * scalar);

            public double Dot(Point3D other) => X * other.X + Y * other.Y + Z * other.Z;
            public Point3D Cross(Point3D other) => new Point3D(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X);
            public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);
            public Point3D Normalize() => this * (1.0 / Length());
        }

        public struct Line2D
        {
            public Point3D Start;
            public Point3D End;
            public Line2D(Point3D start, Point3D end) { Start = start; End = end; }
        }

        public struct Face
        {
            public List<Point3D> Vertices;
            public Point3D Normal;

            public Face(List<Point3D> vertices)
            {
                Vertices = vertices;
                Normal = CalculateNormal(vertices);
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
                return IsPointInPolygon(point, Vertices, Normal);
            }

            private double DistanceToPlane(Point3D point)
            {
                // Plane equation: (point - vertices[0]) · normal = 0
                return (point - Vertices[0]).Dot(Normal);
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

        public struct Point2D
        {
            public double X, Y;
            public Point2D(double x, double y) { X = x; Y = y; }
        }

        public struct BRepSolid
        {
            public List<Face> Faces;

            public BRepSolid(List<Face> faces)
            {
                Faces = faces;
            }
        }

        // Main method to check if line is contained in B-Rep solid
        public static bool ContainsLine(BRepSolid solid, Line2D line, double tolerance = 1e-10)
        {
            // Check if both endpoints are inside the solid
            if (!IsPointInsideSolid(solid, line.Start, tolerance) ||
                !IsPointInsideSolid(solid, line.End, tolerance))
                return false;

            // Check if the entire line segment is inside by sampling multiple points
            int samples = 10; // Increase for more accuracy
            for (int i = 1; i < samples - 1; i++)
            {
                double t = (double)i / (samples - 1);
                Point3D samplePoint = Interpolate(line.Start, line.End, t);

                if (!IsPointInsideSolid(solid, samplePoint, tolerance))
                    return false;
            }

            return true;
        }


        private static bool RayIntersectsFace(Point3D rayOrigin, Point3D rayDirection, Face face, double tolerance)
        {
            // Ray-plane intersection
            double denom = rayDirection.Dot(face.Normal);
            if (Math.Abs(denom) < tolerance)
                return false; // Ray parallel to plane

            Point3D p0 = face.Vertices[0];
            double t = (p0 - rayOrigin).Dot(face.Normal) / denom;

            if (t < tolerance)
                return false; // Intersection behind ray origin

            Point3D intersection = rayOrigin + rayDirection * t;

            // Check if intersection point is within the face polygon
            return face.IsPointOnFace(intersection, tolerance);
        }

        private static Point3D Interpolate(Point3D a, Point3D b, double t)
        {
            return new Point3D(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t
            );
        }

        // Main method to check relationship between two solids
        public static SolidRelationship CheckSolidRelationship(BRepSolid solidA, BRepSolid solidB, double tolerance = 1e-10)
        {
            // Quick bounding box check for early rejection
            if (!BoundingBoxesIntersect(solidA, solidB, tolerance))
                return SolidRelationship.Separate;

            bool aContainsB = ContainsSolid(solidA, solidB, tolerance);
            bool bContainsA = ContainsSolid(solidB, solidA, tolerance);

            if (aContainsB && bContainsA)
                return SolidRelationship.Equal;
            else if (aContainsB)
                return SolidRelationship.Contains;
            else if (bContainsA)
                return SolidRelationship.ContainedBy;

            // Check for intersection
            if (SolidsIntersect(solidA, solidB, tolerance))
                return SolidRelationship.Intersecting;

            // Check for touching
            if (SolidsTouching(solidA, solidB, tolerance))
                return SolidRelationship.Touching;

            return SolidRelationship.Separate;
        }

        // Check if solidA completely contains solidB
        public static bool ContainsSolid(BRepSolid solidA, BRepSolid solidB, double tolerance = 1e-10)
        {
            // Check if all vertices of solidB are inside solidA
            foreach (var face in solidB.Faces)
            {
                foreach (var vertex in face.Vertices)
                {
                    if (!IsPointInsideSolid(solidA, vertex, tolerance))
                        return false;
                }
            }

            // Additional check: sample points along edges to ensure complete containment
            foreach (var face in solidB.Faces)
            {
                int vertexCount = face.Vertices.Count;
                for (int i = 0; i < vertexCount; i++)
                {
                    Point3D start = face.Vertices[i];
                    Point3D end = face.Vertices[(i + 1) % vertexCount];

                    // Sample points along the edge
                    for (int j = 1; j < 5; j++) // Sample 3 points along each edge
                    {
                        double t = j / 5.0;
                        Point3D samplePoint = Interpolate(start, end, t);
                        if (!IsPointInsideSolid(solidA, samplePoint, tolerance))
                            return false;
                    }
                }
            }

            return true;
        }

        // Check if solids intersect (cross each other)
        public static bool SolidsIntersect(BRepSolid solidA, BRepSolid solidB, double tolerance = 1e-10)
        {
            // Check for face-face intersections
            foreach (var faceA in solidA.Faces)
            {
                foreach (var faceB in solidB.Faces)
                {
                    if (FacesIntersect(faceA, faceB, tolerance))
                        return true;
                }
            }

            // Check if any vertex of solidA is inside solidB and vice versa
            bool vertexAInsideB = solidA.Faces.Any(face =>
                face.Vertices.Any(vertex => IsPointInsideSolid(solidB, vertex, tolerance)));

            bool vertexBInsideA = solidB.Faces.Any(face =>
                face.Vertices.Any(vertex => IsPointInsideSolid(solidA, vertex, tolerance)));

            return vertexAInsideB || vertexBInsideA;
        }

        // Check if solids are touching (surfaces contact but no penetration)
        public static bool SolidsTouching(BRepSolid solidA, BRepSolid solidB, double tolerance = 1e-10)
        {
            // Check for face-face contact without penetration
            foreach (var faceA in solidA.Faces)
            {
                foreach (var faceB in solidB.Faces)
                {
                    if (FacesTouching(faceA, faceB, tolerance))
                    {
                        // Ensure no penetration by checking vertices
                        bool penetration = solidA.Faces.Any(face =>
                            face.Vertices.Any(vertex => IsPointInsideSolid(solidB, vertex, tolerance))) ||
                                          solidB.Faces.Any(face =>
                            face.Vertices.Any(vertex => IsPointInsideSolid(solidA, vertex, tolerance)));

                        if (!penetration)
                            return true;
                    }
                }
            }
            return false;
        }

        // Check if two faces intersect
        private static bool FacesIntersect(Face faceA, Face faceB, double tolerance)
        {
            // Check for edge-face intersections
            if (FaceEdgesIntersectFace(faceA, faceB, tolerance) ||
                FaceEdgesIntersectFace(faceB, faceA, tolerance))
                return true;

            return false;
        }

        // Check if any edge of faceA intersects faceB
        private static bool FaceEdgesIntersectFace(Face faceA, Face faceB, double tolerance)
        {
            int vertexCount = faceA.Vertices.Count;
            for (int i = 0; i < vertexCount; i++)
            {
                Point3D start = faceA.Vertices[i];
                Point3D end = faceA.Vertices[(i + 1) % vertexCount];

                if (EdgeIntersectsFace(start, end, faceB, tolerance))
                    return true;
            }
            return false;
        }

        // Check if an edge intersects a face
        private static bool EdgeIntersectsFace(Point3D start, Point3D end, Face face, double tolerance)
        {
            Point3D edgeDirection = (end - start).Normalize();
            double edgeLength = (end - start).Length();

            // Ray-plane intersection
            double denom = edgeDirection.Dot(face.Normal);
            if (Math.Abs(denom) < tolerance)
                return false; // Edge parallel to face plane

            Point3D p0 = face.Vertices[0];
            double t = (p0 - start).Dot(face.Normal) / denom;

            if (t < -tolerance || t > edgeLength + tolerance)
                return false; // Intersection outside edge segment

            Point3D intersection = start + edgeDirection * t;

            // Check if intersection point is within the face polygon
            return face.IsPointOnFace(intersection, tolerance);
        }

        // Check if two faces are touching (surface contact)
        private static bool FacesTouching(Face faceA, Face faceB, double tolerance)
        {
            // Check if faces are coplanar and overlapping
            if (!AreFacesCoplanar(faceA, faceB, tolerance))
                return false;

            // Check if any vertex of faceA lies on faceB or vice versa
            bool touching = faceA.Vertices.Any(vertex => faceB.IsPointOnFace(vertex, tolerance)) ||
                           faceB.Vertices.Any(vertex => faceA.IsPointOnFace(vertex, tolerance));

            if (!touching)
                return false;

            // Ensure faces don't actually intersect (only touch at boundaries)
            return !FacesIntersect(faceA, faceB, tolerance * 10); // Use larger tolerance to avoid false positives
        }

        // Check if two faces are coplanar
        private static bool AreFacesCoplanar(Face faceA, Face faceB, double tolerance)
        {
            // Check if normals are parallel (or anti-parallel)
            double dotProduct = Math.Abs(faceA.Normal.Dot(faceB.Normal));
            if (Math.Abs(dotProduct - 1.0) > tolerance)
                return false;

            // Check if a point from faceB lies on faceA's plane
            Point3D testPoint = faceB.Vertices[0];
            double distance = Math.Abs((testPoint - faceA.Vertices[0]).Dot(faceA.Normal));
            return distance < tolerance;
        }

        // Bounding box check for early rejection
        private static bool BoundingBoxesIntersect(BRepSolid solidA, BRepSolid solidB, double tolerance)
        {
            var bboxA = CalculateBoundingBox(solidA);
            var bboxB = CalculateBoundingBox(solidB);

            // Check for separation along any axis
            if (bboxA.MaxX < bboxB.MinX - tolerance || bboxA.MinX > bboxB.MaxX + tolerance)
                return false;
            if (bboxA.MaxY < bboxB.MinY - tolerance || bboxA.MinY > bboxB.MaxY + tolerance)
                return false;
            if (bboxA.MaxZ < bboxB.MinZ - tolerance || bboxA.MinZ > bboxB.MaxZ + tolerance)
                return false;

            return true;
        }

        private struct BoundingBox
        {
            public double MinX, MaxX, MinY, MaxY, MinZ, MaxZ;
        }

        private static BoundingBox CalculateBoundingBox(BRepSolid solid)
        {
            var allVertices = solid.Faces.SelectMany(face => face.Vertices).ToList();

            return new BoundingBox
            {
                MinX = allVertices.Min(v => v.X),
                MaxX = allVertices.Max(v => v.X),
                MinY = allVertices.Min(v => v.Y),
                MaxY = allVertices.Max(v => v.Y),
                MinZ = allVertices.Min(v => v.Z),
                MaxZ = allVertices.Max(v => v.Z)
            };
        }

        // ... (Previous methods: IsPointInsideSolid, RayIntersectsFace, Interpolate remain the same)

        // Enhanced version of IsPointInsideSolid from previous code
        private static bool IsPointInsideSolid(BRepSolid solid, Point3D point, double tolerance)
        {
            // Use multiple rays in different directions for robustness
            Point3D[] rayDirections = new Point3D[]
            {
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0),
            new Point3D(0, 0, 1),
            new Point3D(1, 1, 1).Normalize()
            };

            foreach (var rayDir in rayDirections)
            {
                int intersections = CountRayIntersections(solid, point, rayDir, tolerance);
                if (intersections % 2 == 1)
                    return true;
            }

            return false;
        }

        private static int CountRayIntersections(BRepSolid solid, Point3D rayOrigin, Point3D rayDirection, double tolerance)
        {
            int intersections = 0;

            foreach (var face in solid.Faces)
            {
                if (RayIntersectsFace(rayOrigin, rayDirection, face, tolerance))
                    intersections++;
            }

            return intersections;
        }

        // Example usage with comprehensive testing
        public static void Example()
        {
            // Create two cube B-Reps
            var cube1 = CreateCube(new Point3D(0, 0, 0), 1.0);
            var cube2 = CreateCube(new Point3D(0.5, 0.5, 0.5), 1.0); // Overlapping cube

            // Check relationship
            var relationship = CheckSolidRelationship(cube1, cube2);

            Console.WriteLine($"Solid relationship: {relationship}");

            // Test specific conditions
            bool contains = ContainsSolid(cube1, cube2);
            bool intersects = SolidsIntersect(cube1, cube2);

            Console.WriteLine($"Cube1 contains Cube2: {contains}");
            Console.WriteLine($"Cubes intersect: {intersects}");
        }

        // Helper method to create a cube B-Rep
        private static BRepSolid CreateCube(Point3D center, double size)
        {
            double halfSize = size / 2;
            List<Face> faces = new List<Face>();

            // Define cube faces (simplified - you'd want to complete all 6 faces)
            // Front face
            faces.Add(new Face(new List<Point3D>
        {
            new Point3D(center.X - halfSize, center.Y - halfSize, center.Z - halfSize),
            new Point3D(center.X + halfSize, center.Y - halfSize, center.Z - halfSize),
            new Point3D(center.X + halfSize, center.Y + halfSize, center.Z - halfSize),
            new Point3D(center.X - halfSize, center.Y + halfSize, center.Z - halfSize)
        }));

            // Back face
            faces.Add(new Face(new List<Point3D>
        {
            new Point3D(center.X - halfSize, center.Y - halfSize, center.Z + halfSize),
            new Point3D(center.X + halfSize, center.Y - halfSize, center.Z + halfSize),
            new Point3D(center.X + halfSize, center.Y + halfSize, center.Z + halfSize),
            new Point3D(center.X - halfSize, center.Y + halfSize, center.Z + halfSize)
        }));

            // Add other 4 faces for complete cube...
            // Left, Right, Top, Bottom faces would be added here

            return new BRepSolid(faces);
        }
    }

}

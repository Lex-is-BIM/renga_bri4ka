using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public enum SolidRelationship
    {
        Separate,      // Solids don't touch or intersect
        Touching,      // Solids touch but don't penetrate
        Intersecting,  // Solids cross each other
        Contains,      // First solid completely contains second
        ContainedBy,   // First solid is completely contained by second
        Equal          // Solids are identical (within tolerance)
    }

    public class FacetedBRepSolid
    {
        public Dictionary<int, Point3D> Points { get; private set; }

        public List<Face> Faces { get; private set; }

        public BoundingBox? BBox { get; private set; }

        public FacetedBRepSolid()
        {
            Points = new Dictionary<int, Point3D>();
            Faces = new List<Face>();
        }

        public int AddPoint(Point3D point3d)
        {
            Points.Add(Points.Count, point3d);
            return Points.Count - 1;
        }

        public void AddFace(Face face)
        {
            Faces.Add(face);
        }

        public List<Point3D> GetPoints(IEnumerable<int> indexes)
        {
            List<Point3D> ps = new List<Point3D>();
            foreach (int index in indexes)
            {
                ps.Add(Points[index]);
            }
            return ps;
        }

        public BoundingBox GetBBox()
        {
            if (this.BBox == null) this.BBox = CalculateBoundingBox(this);
            return this.BBox;
        }


        // Main method to check if line is contained in B-Rep solid
        public static bool ContainsLine(FacetedBRepSolid solid, Line2D line, double tolerance = 1e-10)
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

            Point3D p0 = face.Owner.Points[face.Vertices[0]];
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
        public static SolidRelationship CheckSolidRelationship(FacetedBRepSolid solidA, FacetedBRepSolid solidB, double tolerance = 1e-10)
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
        public static bool ContainsSolid(FacetedBRepSolid solidA, FacetedBRepSolid solidB, double tolerance = 1e-10)
        {
            // Check if all vertices of solidB are inside solidA
            foreach (var face in solidB.Faces)
            {
                foreach (var vertex in face.Owner.GetPoints(face.Vertices))
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
                    Point3D start = face.Owner.Points[face.Vertices[i]];
                    Point3D end = face.Owner.Points[face.Vertices[(i + 1) % vertexCount]];

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
        public static bool SolidsIntersect(FacetedBRepSolid solidA, FacetedBRepSolid solidB, double tolerance = 1e-10)
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
                face.Vertices.Any(vertex => IsPointInsideSolid(solidB, face.Owner.Points[vertex], tolerance)));

            bool vertexBInsideA = solidB.Faces.Any(face =>
                face.Vertices.Any(vertex => IsPointInsideSolid(solidA, face.Owner.Points[vertex], tolerance)));

            return vertexAInsideB || vertexBInsideA;
        }

        // Check if solids are touching (surfaces contact but no penetration)
        public static bool SolidsTouching(FacetedBRepSolid solidA, FacetedBRepSolid solidB, double tolerance = 1e-10)
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
                            face.Vertices.Any(vertex => IsPointInsideSolid(solidB, face.Owner.Points[vertex], tolerance))) ||
                                          solidB.Faces.Any(face =>
                            face.Vertices.Any(vertex => IsPointInsideSolid(solidA, face.Owner.Points[vertex], tolerance)));

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
                Point3D start = faceA.Owner.Points[faceA.Vertices[i]];
                Point3D end = faceA.Owner.Points[faceA.Vertices[(i + 1) % vertexCount]];

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

            Point3D p0 = face.Owner.Points[face.Vertices[0]];
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
            bool touching = faceA.Vertices.Any(vertex => faceB.IsPointOnFace(faceB.Owner.Points[vertex], tolerance)) ||
                           faceB.Vertices.Any(vertex => faceA.IsPointOnFace(faceA.Owner.Points[vertex], tolerance));

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
            Point3D testPoint = faceB.Owner.Points[faceB.Vertices[0]];
            double distance = Math.Abs((testPoint - faceA.Owner.Points[faceB.Vertices[0]]).Dot(faceA.Normal));
            return distance < tolerance;
        }

        // Bounding box check for early rejection
        private static bool BoundingBoxesIntersect(FacetedBRepSolid solidA, FacetedBRepSolid solidB, double tolerance)
        {
            var bboxA = solidA.GetBBox();
            var bboxB = solidB.GetBBox();

            // Check for separation along any axis
            if (bboxA.MaxX < bboxB.MinX - tolerance || bboxA.MinX > bboxB.MaxX + tolerance)
                return false;
            if (bboxA.MaxY < bboxB.MinY - tolerance || bboxA.MinY > bboxB.MaxY + tolerance)
                return false;
            if (bboxA.MaxZ < bboxB.MinZ - tolerance || bboxA.MinZ > bboxB.MaxZ + tolerance)
                return false;

            return true;
        }

        private static BoundingBox CalculateBoundingBox(FacetedBRepSolid solid)
        {
            var allVertices = solid.Points.Select(p => p.Value).ToList();

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
        private static bool IsPointInsideSolid(FacetedBRepSolid solid, Point3D point, double tolerance)
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

        private static int CountRayIntersections(FacetedBRepSolid solid, Point3D rayOrigin, Point3D rayDirection, double tolerance)
        {
            int intersections = 0;

            foreach (var face in solid.Faces)
            {
                if (RayIntersectsFace(rayOrigin, rayDirection, face, tolerance))
                    intersections++;
            }

            return intersections;
        }
    }
}

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

    internal class FacetedBRepSolid
    {
        public Dictionary<int, Point3D> Points { get; private set; }
        public Dictionary<int, Face> Faces { get; private set; }
        public BoundingBox? BBox { get; private set; }

        public FacetedBRepSolid()
        {
            Points = new Dictionary<int, Point3D>();
            Faces = new Dictionary<int, Face>();
        }

        public int AddPoint(Point3D point3d)
        {
            Points.Add(Points.Count, point3d);
            return Points.Count - 1;
        }

        public void AddFace(Face face)
        {
            Faces.Add(Faces.Count, face);
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

        // Main method to check if line is contained in B-Rep solid
        public bool ContainsLine(Line2D line, double tolerance = 1e-10)
        {
            // Check if both endpoints are inside the solid
            if (!IsPointInsideSolid(line.Start, tolerance) ||
                !IsPointInsideSolid(line.End, tolerance))
                return false;

            // Check if the entire line segment is inside by sampling multiple points
            int samples = 10; // Increase for more accuracy
            for (int i = 1; i < samples - 1; i++)
            {
                double t = (double)i / (samples - 1);
                Point3D samplePoint = Interpolate(line.Start, line.End, t);

                if (!IsPointInsideSolid(samplePoint, tolerance))
                    return false;
            }

            return true;
        }

        private bool IsPointInsideSolid(Point3D point, double tolerance)
        {
            // Use ray casting method - shoot a ray and count intersections
            int intersections = 0;
            Point3D rayDirection = new Point3D(1, 0, 0); // Arbitrary direction

            foreach (var face in this.Faces)
            {
                if (RayIntersectsFace(point, rayDirection, face.Value, tolerance))
                    intersections++;
            }

            return intersections % 2 == 1; // Odd number of intersections = inside
        }

        private bool RayIntersectsFace(Point3D rayOrigin, Point3D rayDirection, Face face, double tolerance)
        {
            // Ray-plane intersection
            double denom = rayDirection.Dot(face.Normal);
            if (Math.Abs(denom) < tolerance)
                return false; // Ray parallel to plane

            Point3D p0 = this.Points[face.Vertices[0]];
            double t = (p0 - rayOrigin).Dot(face.Normal) / denom;

            if (t < tolerance)
                return false; // Intersection behind ray origin

            Point3D intersection = rayOrigin + rayDirection * t;

            // Check if intersection point is within the face polygon
            return IsPointOnFace(intersection, face, tolerance);
        }

        private Point3D Interpolate(Point3D a, Point3D b, double t)
        {
            return new Point3D(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t
            );
        }

        public bool IsPointOnFace(Point3D point, Face face, double tolerance = 1e-10)
        {
            // Check if point is coplanar with the face
            if (Math.Abs(DistanceToPlane(point, face)) > tolerance)
                return false;

            // Use point-in-polygon test with ray casting
            return Face.IsPointInPolygon(point, GetPoints(face.Vertices), face.Normal);
        }

        private double DistanceToPlane(Point3D point, Face face)
        {
            // Plane equation: (point - vertices[0]) · normal = 0
            return (point - Points[face.Vertices[0]]).Dot(face.Normal);
        }

        // Main method to check relationship between two solids
        public SolidRelationship CheckSolidRelationship(FacetedBRepSolid solidB, double tolerance = 1e-10)
        {
            // Quick bounding box check for early rejection
            if (!BoundingBoxesIntersect(solidB, tolerance))
                return SolidRelationship.Separate;

            bool aContainsB = ContainsSolid(solidB, tolerance);
            bool bContainsA = solidB.ContainsSolid(this, tolerance);

            if (aContainsB && bContainsA)
                return SolidRelationship.Equal;
            else if (aContainsB)
                return SolidRelationship.Contains;
            else if (bContainsA)
                return SolidRelationship.ContainedBy;

            // Check for intersection
            if (SolidsIntersect(solidB, tolerance))
                return SolidRelationship.Intersecting;

            // Check for touching
            if (SolidsTouching(solidB, tolerance))
                return SolidRelationship.Touching;

            return SolidRelationship.Separate;
        }

        // Check if solidA completely contains solidB
        public bool ContainsSolid(FacetedBRepSolid solidB, double tolerance = 1e-10)
        {
            // Check if all vertices of solidB are inside solidA
            foreach (var face in solidB.Faces)
            {
                foreach (var vertex in GetPoints(face.Value.Vertices))
                {
                    if (!IsPointInsideSolid(this, vertex, tolerance))
                        return false;
                }
            }

            // Additional check: sample points along edges to ensure complete containment
            foreach (var face in solidB.Faces)
            {
                int vertexCount = face.Value.Vertices.Count();
                for (int i = 0; i < vertexCount; i++)
                {
                    Point3D start = Points[face.Value.Vertices[i]];
                    Point3D end = Points[face.Value.Vertices[(i + 1) % vertexCount]];

                    // Sample points along the edge
                    for (int j = 1; j < 5; j++) // Sample 3 points along each edge
                    {
                        double t = j / 5.0;
                        Point3D samplePoint = Interpolate(start, end, t);
                        if (!IsPointInsideSolid(this, samplePoint, tolerance))
                            return false;
                    }
                }
            }

            return true;
        }

        // Check if solids intersect (cross each other)
        public bool SolidsIntersect(FacetedBRepSolid solidB, double tolerance = 1e-10)
        {
            // Check for face-face intersections
            foreach (var faceA in this.Faces)
            {
                foreach (var faceB in solidB.Faces)
                {
                    if (FacesIntersect(faceA.Value, faceB.Value, solidB, tolerance))
                        return true;
                }
            }

            // Check if any vertex of solidA is inside solidB and vice versa
            bool vertexAInsideB = this.Faces.Any(face =>
                face.Value.Vertices.Any(vertex => solidB.IsPointInsideSolid(Points[vertex], tolerance)));

            bool vertexBInsideA = solidB.Faces.Any(face =>
                face.Value.Vertices.Any(vertex => IsPointInsideSolid(Points[vertex], tolerance)));

            return vertexAInsideB || vertexBInsideA;
        }

        // Check if solids are touching (surfaces contact but no penetration)
        public bool SolidsTouching(FacetedBRepSolid solidB, double tolerance = 1e-10)
        {
            // Check for face-face contact without penetration
            foreach (var faceA in this.Faces)
            {
                foreach (var faceB in solidB.Faces)
                {
                    if (FacesTouching(faceA.Value, faceB.Value, solidB, tolerance))
                    {
                        // Ensure no penetration by checking vertices
                        bool penetration = this.Faces.Any(face =>
                            GetPoints(face.Value.Vertices).Any(vertex => IsPointInsideSolid(solidB, vertex, tolerance))) ||
                                          solidB.Faces.Any(face =>
                            GetPoints(face.Value.Vertices).Any(vertex => IsPointInsideSolid(this, vertex, tolerance)));

                        if (!penetration)
                            return true;
                    }
                }
            }
            return false;
        }

        // Check if two faces intersect
        private bool FacesIntersect(Face faceA, Face faceB, FacetedBRepSolid solidB, double tolerance)
        {
            // Check for edge-face intersections
            if (FaceEdgesIntersectFace(faceA, faceB, solidB, tolerance) ||
                FaceEdgesIntersectFace(faceB, faceA, solidB, tolerance))
                return true;

            return false;
        }

        // Check if any edge of faceA intersects faceB
        private bool FaceEdgesIntersectFace(Face faceA, Face faceB, FacetedBRepSolid solidB, double tolerance)
        {
            int vertexCount = faceA.Vertices.Count();
            for (int i = 0; i < vertexCount; i++)
            {
                Point3D start = Points[faceA.Vertices[i]];
                Point3D end = Points[faceA.Vertices[(i + 1) % vertexCount]];

                if (EdgeIntersectsFace(start, end, faceB, solidB, tolerance))
                    return true;
            }
            return false;
        }

        // Check if an edge intersects a face
        private bool EdgeIntersectsFace(Point3D start, Point3D end, Face face, FacetedBRepSolid solidB, double tolerance)
        {
            Point3D edgeDirection = (end - start).Normalize();
            double edgeLength = (end - start).Length();

            // Ray-plane intersection
            double denom = edgeDirection.Dot(face.Normal);
            if (Math.Abs(denom) < tolerance)
                return false; // Edge parallel to face plane

            Point3D p0 = solidB.Points[face.Vertices[0]];
            double t = (p0 - start).Dot(face.Normal) / denom;

            if (t < -tolerance || t > edgeLength + tolerance)
                return false; // Intersection outside edge segment

            Point3D intersection = start + edgeDirection * t;

            // Check if intersection point is within the face polygon
            return IsPointOnFace(intersection, face, tolerance);
        }

        // Check if two faces are touching (surface contact)
        private bool FacesTouching(Face faceA, Face faceB, FacetedBRepSolid solidB, double tolerance)
        {
            // Check if faces are coplanar and overlapping
            if (!AreFacesCoplanar(faceA, faceB, solidB, tolerance))
                return false;

            // Check if any vertex of faceA lies on faceB or vice versa
            bool touching = faceA.Vertices.Any(vertex => IsPointOnFace(Points[vertex], faceB, tolerance)) ||
                           faceB.Vertices.Any(vertex => IsPointOnFace(Points[vertex], faceA, tolerance));

            if (!touching)
                return false;

            // Ensure faces don't actually intersect (only touch at boundaries)
            return !FacesIntersect(faceA, faceB, solidB, tolerance * 10); // Use larger tolerance to avoid false positives
        }

        // Check if two faces are coplanar
        private bool AreFacesCoplanar(Face faceA, Face faceB, FacetedBRepSolid solidB, double tolerance)
        {
            // Check if normals are parallel (or anti-parallel)
            double dotProduct = Math.Abs(faceA.Normal.Dot(faceB.Normal));
            if (Math.Abs(dotProduct - 1.0) > tolerance)
                return false;

            // Check if a point from faceB lies on faceA's plane
            Point3D testPoint = this.Points[faceB.Vertices[0]];
            double distance = Math.Abs((testPoint - this.Points[faceA.Vertices[0]]).Dot(faceA.Normal));
            return distance < tolerance;
        }

        // Bounding box check for early rejection
        private bool BoundingBoxesIntersect(FacetedBRepSolid solidB, double tolerance)
        {
            if (this.BBox == null) CalculateBoundingBox();
            if (solidB.BBox == null) solidB.CalculateBoundingBox();

            BoundingBox solid1BBox = (BoundingBox)this.BBox;
            BoundingBox solid2BBox = (BoundingBox)solidB.BBox;
            // Check for separation along any axis
            if (solid1BBox.MaxX < solid2BBox.MinX - tolerance || solid1BBox.MinX > solid2BBox.MaxX + tolerance)
                return false;
            if (solid1BBox.MaxY < solid2BBox.MinY - tolerance || solid1BBox.MinY > solid2BBox.MaxY + tolerance)
                return false;
            if (solid1BBox.MaxZ < solid2BBox.MinZ - tolerance || solid1BBox.MinZ > solid2BBox.MaxZ + tolerance)
                return false;

            return true;
        }

        public struct BoundingBox
        {
            public double MinX, MaxX, MinY, MaxY, MinZ, MaxZ;
        }

        public void CalculateBoundingBox()
        {
            var allVertices = this.Points.Select(p=>p.Value).ToList();

            this.BBox = new BoundingBox
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
        private bool IsPointInsideSolid(FacetedBRepSolid solid, Point3D point, double tolerance)
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
                int intersections = CountRayIntersections(point, rayDir, tolerance);
                if (intersections % 2 == 1)
                    return true;
            }

            return false;
        }

        private int CountRayIntersections(Point3D rayOrigin, Point3D rayDirection, double tolerance)
        {
            int intersections = 0;

            foreach (var face in this.Faces)
            {
                if (RayIntersectsFace(rayOrigin, rayDirection, face.Value, tolerance))
                    intersections++;
            }

            return intersections;
        }

    }
}

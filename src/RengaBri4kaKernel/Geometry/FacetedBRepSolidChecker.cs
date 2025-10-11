using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public class FacetedBRepSolidChecker
    {
        // Main method to check if line is contained in B-Rep solid
        public static SolidRelationship ContainsLine(FacetedBRepSolid solid, Line3D? line, double tolerance = 1e-10)
        {
            if (line == null) return SolidRelationship._Error;
            // Check if both endpoints are inside the solid
            foreach (var point in line.Vertices)
            {
                if (!IsPointInsideSolid(solid, point, tolerance)) return SolidRelationship.Contains;
            }

            return SolidRelationship.Separate;
        }


        private static bool RayIntersectsFace(Vector3 rayOrigin, Vector3 rayDirection, Face face, double tolerance)
        {
            // Ray-plane intersection
            double denom = rayDirection.Dot(face.Normal);
            if (Math.Abs(denom) < tolerance)
                return false; // Ray parallel to plane

            Vector3 p0 = face.Vertices[0];
            double t = (p0 - rayOrigin).Dot(face.Normal) / denom;

            if (t < tolerance)
                return false; // Intersection behind ray origin

            Vector3 intersection = rayOrigin + rayDirection * t;

            // Check if intersection point is within the face polygon
            return face.IsPointOnFace(intersection, tolerance);
        }

        private static Vector3 Interpolate(Vector3 a, Vector3 b, double t)
        {
            return new Vector3(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t
            );
        }

        // Main method to check relationship between two solids
        public static SolidRelationship CheckSolidRelationship(FacetedBRepSolid? solidA, FacetedBRepSolid? solidB, double tolerance = 1e-10)
        {
            if (solidA == null) return SolidRelationship._Error;
            if (solidB == null) return SolidRelationship._Error;

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
                    Vector3 start = face.Vertices[i];
                    Vector3 end = face.Vertices[(i + 1) % vertexCount];

                    // Sample points along the edge
                    for (int j = 1; j < 5; j++) // Sample 3 points along each edge
                    {
                        double t = j / 5.0;
                        Vector3 samplePoint = Interpolate(start, end, t);
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
                face.Vertices.Any(vertex => IsPointInsideSolid(solidB, vertex, tolerance)));

            bool vertexBInsideA = solidB.Faces.Any(face =>
                face.Vertices.Any(vertex => IsPointInsideSolid(solidA, vertex, tolerance)));

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
                Vector3 start = faceA.Vertices[i];
                Vector3 end = faceA.Vertices[(i + 1) % vertexCount];

                if (EdgeIntersectsFace(start, end, faceB, tolerance))
                    return true;
            }
            return false;
        }

        // Check if an edge intersects a face
        private static bool EdgeIntersectsFace(Vector3 start, Vector3 end, Face face, double tolerance)
        {
            Vector3 edgeDirection = (end - start).Normalized();
            double edgeLength = (end - start).LengthSquared();

            // Ray-plane intersection
            double denom = edgeDirection.Dot(face.Normal);
            if (Math.Abs(denom) < tolerance)
                return false; // Edge parallel to face plane

            Vector3 p0 = face.Vertices[0];
            double t = (p0 - start).Dot(face.Normal) / denom;

            if (t < -tolerance || t > edgeLength + tolerance)
                return false; // Intersection outside edge segment

            Vector3 intersection = start + edgeDirection * t;

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
            Vector3 testPoint = faceB.Vertices[0];
            double distance = Math.Abs((testPoint - faceA.Vertices[0]).Dot(faceA.Normal));
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

        

        // ... (Previous methods: IsPointInsideSolid, RayIntersectsFace, Interpolate remain the same)

        // Enhanced version of IsPointInsideSolid from previous code
        private static bool IsPointInsideSolid(FacetedBRepSolid solid, Vector3 point, double tolerance)
        {
            // Use multiple rays in different directions for robustness
            Vector3[] rayDirections = new Vector3[]
            {
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 1, 1).Normalized()
            };

            foreach (var rayDir in rayDirections)
            {
                int intersections = CountRayIntersections(solid, point, rayDir, tolerance);
                if (intersections % 2 == 1)
                    return true;
            }

            return false;
        }

        private static int CountRayIntersections(FacetedBRepSolid solid, Vector3 rayOrigin, Vector3 rayDirection, double tolerance)
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

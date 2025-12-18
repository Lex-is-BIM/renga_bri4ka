using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public class MultiPlanarOptimizer
    {
        private const double Tolerance = 1e-10;

        public static FacetedBRepSolid OptimizeTrianglesToBrep(List<Triangle2> triangles)
        {
            if (triangles == null || triangles.Count == 0)
                return new FacetedBRepSolid();

            // Step 1: Group triangles by their planes
            var planeGroups = GroupTrianglesByPlanes(triangles);

            Console.WriteLine($"Found {planeGroups.Count} distinct planes");

            // Step 2: Optimize each plane group into faces
            var resultBrep = new FacetedBRepSolid();

            foreach (var group in planeGroups)
            {
                Console.WriteLine($"Processing plane with normal {group.Key.Normal}, containing {group.Value.Count} triangles");

                var optimizedFaces = OptimizePlanarTriangles(group.Value, group.Key);

                foreach (var face in optimizedFaces)
                {
                    resultBrep.AddFace(face, group.Key.Normal);
                }
            }

            return resultBrep;
        }

        private static Dictionary<Plane, List<Triangle2>> GroupTrianglesByPlanes(List<Triangle2> triangles)
        {
            var groups = new Dictionary<Plane, List<Triangle2>>();

            foreach (var triangle in triangles)
            {
                var plane = new Plane(triangle.V1, triangle.V2, triangle.V3);
                bool foundGroup = false;

                // Check if this triangle belongs to any existing plane group
                foreach (var existingPlane in groups.Keys.ToList())
                {
                    if (existingPlane.Equals(plane) &&
                        existingPlane.IsPointOnPlane(triangle.V1) &&
                        existingPlane.IsPointOnPlane(triangle.V2) &&
                        existingPlane.IsPointOnPlane(triangle.V3))
                    {
                        groups[existingPlane].Add(triangle);
                        foundGroup = true;
                        break;
                    }
                }

                if (!foundGroup)
                {
                    groups[plane] = new List<Triangle2> { triangle };
                }
            }

            return groups;
        }

        private static List<List<Vector3>> OptimizePlanarTriangles(List<Triangle2> triangles, Plane plane)
        {
            if (triangles.Count == 0)
                return new List<List<Vector3>>();

            // For now, we'll create one face per connected component
            // In a more advanced implementation, you could handle holes and multiple contours

            var allEdges = ExtractAllEdges(triangles);
            var boundaryEdges = FindBoundaryEdges(allEdges);
            var connectedComponents = FindConnectedComponents(boundaryEdges);

            var faces = new List<List<Vector3>>();

            foreach (var component in connectedComponents)
            {
                var polygon = ReconstructBoundaryPolygon(component);
                if (polygon.Count >= 3)
                {
                    faces.Add(polygon);
                }
            }

            Console.WriteLine($"  Created {faces.Count} faces from {triangles.Count} triangles");

            return faces;
        }

        private static List<Edge> ExtractAllEdges(List<Triangle2> triangles)
        {
            var edges = new List<Edge>();

            foreach (var triangle in triangles)
            {
                edges.Add(new Edge(triangle.V1, triangle.V2));
                edges.Add(new Edge(triangle.V2, triangle.V3));
                edges.Add(new Edge(triangle.V3, triangle.V1));
            }

            return edges;
        }

        private static List<Edge> FindBoundaryEdges(List<Edge> allEdges)
        {
            var edgeCounts = new Dictionary<Edge, int>();

            foreach (var edge in allEdges)
            {
                var normalizedEdge = edge.Normalized();
                if (edgeCounts.ContainsKey(normalizedEdge))
                {
                    edgeCounts[normalizedEdge]++;
                }
                else
                {
                    edgeCounts[normalizedEdge] = 1;
                }
            }

            return edgeCounts
                .Where(kvp => kvp.Value == 1) // Edges that appear only once are boundary edges
                .Select(kvp => kvp.Key)
                .ToList();
        }

        private static List<List<Edge>> FindConnectedComponents(List<Edge> edges)
        {
            var components = new List<List<Edge>>();
            var remainingEdges = new HashSet<Edge>(edges);

            while (remainingEdges.Count > 0)
            {
                var component = new List<Edge>();
                var currentEdge = remainingEdges.First();
                remainingEdges.Remove(currentEdge);
                component.Add(currentEdge);

                // Grow component by finding connected edges
                bool foundNewEdge;
                do
                {
                    foundNewEdge = false;
                    foreach (var edge in remainingEdges.ToList())
                    {
                        if (IsConnectedToComponent(edge, component))
                        {
                            component.Add(edge);
                            remainingEdges.Remove(edge);
                            foundNewEdge = true;
                        }
                    }
                } while (foundNewEdge);

                components.Add(component);
            }

            return components;
        }

        private static bool IsConnectedToComponent(Edge edge, List<Edge> component)
        {
            foreach (var componentEdge in component)
            {
                if (ShareVertex(edge, componentEdge))
                    return true;
            }
            return false;
        }

        private static bool ShareVertex(Edge e1, Edge e2)
        {
            return VectorEquals(e1.Start, e2.Start) || VectorEquals(e1.Start, e2.End) ||
                   VectorEquals(e1.End, e2.Start) || VectorEquals(e1.End, e2.End);
        }

        private static List<Vector3> ReconstructBoundaryPolygon(List<Edge> edges)
        {
            if (edges.Count == 0)
                return new List<Vector3>();

            var polygon = new List<Vector3>();
            var edgeList = new List<Edge>(edges);

            // Start with first edge
            var currentEdge = edgeList[0];
            polygon.Add(currentEdge.Start);
            polygon.Add(currentEdge.End);
            edgeList.RemoveAt(0);

            // Connect edges to form closed polygon
            while (edgeList.Count > 0)
            {
                bool foundNext = false;

                for (int i = 0; i < edgeList.Count; i++)
                {
                    var edge = edgeList[i];

                    if (VectorEquals(edge.Start, polygon[polygon.Count - 1]))
                    {
                        polygon.Add(edge.End);
                        edgeList.RemoveAt(i);
                        foundNext = true;
                        break;
                    }
                    else if (VectorEquals(edge.End, polygon[polygon.Count - 1]))
                    {
                        polygon.Add(edge.Start);
                        edgeList.RemoveAt(i);
                        foundNext = true;
                        break;
                    }
                }

                if (!foundNext)
                {
                    // Try to connect to beginning to close the loop
                    for (int i = 0; i < edgeList.Count; i++)
                    {
                        var edge = edgeList[i];
                        if (VectorEquals(edge.Start, polygon[0]) || VectorEquals(edge.End, polygon[0]))
                        {
                            // We've closed the loop
                            edgeList.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                }
            }

            // Remove last vertex if it's the same as first (closed loop)
            if (polygon.Count > 1 && VectorEquals(polygon[0], polygon[polygon.Count - 1]))
            {
                polygon.RemoveAt(polygon.Count - 1);
            }

            // Ensure polygon has at least 3 vertices
            if (polygon.Count < 3)
                return new List<Vector3>();

            return polygon;
        }

        private static bool VectorEquals(Vector3 a, Vector3 b)
        {
            return (a - b).LengthSquared() < Tolerance;
        }
    }

}

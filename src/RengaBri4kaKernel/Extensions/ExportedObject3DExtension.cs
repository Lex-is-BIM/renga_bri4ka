using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.Extensions
{
    internal static class ExportedObject3DExtension
    {
        public static void GetGeometryStatistics(this Renga.IExportedObject3D geometry, out int meshesCount, out int gridsCount, out int verticesCount, out int trianglesCount, out int normalsCount)
        {
            meshesCount = geometry.MeshCount;
            gridsCount = 0;
            verticesCount = 0;
            trianglesCount = 0;
            normalsCount = 0;

            for (int rengaMeshCounter = 0; rengaMeshCounter < geometry.MeshCount; rengaMeshCounter++)
            {
                Renga.IMesh mesh = geometry.GetMesh(rengaMeshCounter);
                gridsCount += mesh.GridCount;

                for (int rengaGridCounter = 0; rengaGridCounter < mesh.GridCount; rengaGridCounter++)
                {
                    Renga.IGrid grid = mesh.GetGrid(rengaGridCounter);
                    verticesCount += grid.VertexCount;
                    trianglesCount += grid.TriangleCount;
                    normalsCount += grid.NormalCount;
                }
            }
        }

        public static FacetedBRepSolid[] ToFacetedBRep0(this Renga.IExportedObject3D geometry)
        {
            FacetedBRepSolid[] result = new FacetedBRepSolid[geometry.MeshCount];
            for (int rengaMeshCounter = 0; rengaMeshCounter < geometry.MeshCount; rengaMeshCounter++)
            {
                Renga.IMesh mesh = geometry.GetMesh(rengaMeshCounter);

                FacetedBRepSolid brepMesh = new FacetedBRepSolid();
                List<Face> faces = new List<Face>();

                for (int rengaGridCounter = 0; rengaGridCounter < mesh.GridCount; rengaGridCounter++)
                {
                    Renga.IGrid grid = mesh.GetGrid(rengaGridCounter);

                    Dictionary<int, Vector3> verticesIndexMap = new Dictionary<int, Vector3>();
                    for (int rengaVertexCounter = 0; rengaVertexCounter < grid.VertexCount; rengaVertexCounter++)
                    {
                        Renga.FloatPoint3D p = grid.GetVertex(rengaVertexCounter);
                        verticesIndexMap.Add(rengaVertexCounter, new Vector3(p.X, p.Y, p.Z));

                    }
                    for (int rengaFaceCounter = 0; rengaFaceCounter < grid.TriangleCount; rengaFaceCounter++)
                    {
                        Renga.Triangle tr = grid.GetTriangle(rengaFaceCounter);
                        Face f = new Face();
                        f.GetOrAddVertexIndex(verticesIndexMap[(int)tr.V0]);
                        f.GetOrAddVertexIndex(verticesIndexMap[(int)tr.V1]);
                        f.GetOrAddVertexIndex(verticesIndexMap[(int)tr.V2]);
                        f.CalculateNormal();
                        brepMesh.AddFace(f);
                    }
                }
                result[rengaMeshCounter] = brepMesh;
            }
            return result;
        }

        public static FacetedBRepSolid[] ToFacetedBRep(this Renga.IExportedObject3D geometry)
        {
            FacetedBRepSolid[] result = new FacetedBRepSolid[geometry.MeshCount];
            for (int rengaMeshCounter = 0; rengaMeshCounter < geometry.MeshCount; rengaMeshCounter++)
            {
                Renga.IMesh mesh = geometry.GetMesh(rengaMeshCounter);

                
                List<Triangle2> triangles = new List<Triangle2>();

                for (int rengaGridCounter = 0; rengaGridCounter < mesh.GridCount; rengaGridCounter++)
                {
                    Renga.IGrid grid = mesh.GetGrid(rengaGridCounter);

                    Dictionary<int, Vector3> verticesIndexMap = new Dictionary<int, Vector3>();
                    for (int rengaVertexCounter = 0; rengaVertexCounter < grid.VertexCount; rengaVertexCounter++)
                    {
                        Renga.FloatPoint3D p = grid.GetVertex(rengaVertexCounter);
                        verticesIndexMap.Add(rengaVertexCounter, new Vector3(p.X, p.Y, p.Z));

                    }
                    for (int rengaFaceCounter = 0; rengaFaceCounter < grid.TriangleCount; rengaFaceCounter++)
                    {
                        Renga.Triangle tr = grid.GetTriangle(rengaFaceCounter);
                        Triangle2 trDef = new Triangle2(verticesIndexMap[(int)tr.V0], verticesIndexMap[(int)tr.V1], verticesIndexMap[(int)tr.V2]);
                        triangles.Add(trDef);
                    }
                }
                FacetedBRepSolid brepMesh = MultiPlanarOptimizer.OptimizeTrianglesToBrep(triangles);
                result[rengaMeshCounter] = brepMesh;
            }
            return result;
        }

    }
}

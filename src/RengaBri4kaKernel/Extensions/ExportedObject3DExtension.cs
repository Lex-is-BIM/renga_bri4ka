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

        public static BRepContainsLineChecker.BRepSolid[] ToFacetedBRep2(this Renga.IExportedObject3D geometry)
        {
            BRepContainsLineChecker.BRepSolid[] result = new BRepContainsLineChecker.BRepSolid[geometry.MeshCount];
            for (int rengaMeshCounter = 0; rengaMeshCounter < geometry.MeshCount; rengaMeshCounter++)
            {
                Renga.IMesh mesh = geometry.GetMesh(rengaMeshCounter);
                
                List<BRepContainsLineChecker.Face> faces = new List<BRepContainsLineChecker.Face>();

                for (int rengaGridCounter = 0; rengaGridCounter < mesh.GridCount; rengaGridCounter++)
                {
                    Renga.IGrid grid = mesh.GetGrid(rengaGridCounter);

                    Dictionary<int, BRepContainsLineChecker.Point3D> verticesIndexMap = new Dictionary<int, BRepContainsLineChecker.Point3D>();
                    for (int rengaVertexCounter = 0; rengaVertexCounter < grid.VertexCount; rengaVertexCounter++)
                    {
                        Renga.FloatPoint3D p = grid.GetVertex(rengaVertexCounter);
                        verticesIndexMap.Add(rengaVertexCounter, new BRepContainsLineChecker.Point3D(p.X, p.Y, p.Z));

                    }
                    for (int rengaFaceCounter = 0; rengaFaceCounter < grid.TriangleCount; rengaFaceCounter++)
                    {
                        Renga.Triangle tr = grid.GetTriangle(rengaFaceCounter);
                        BRepContainsLineChecker.Face f = new BRepContainsLineChecker.Face(new List<BRepContainsLineChecker.Point3D> {
                        verticesIndexMap[(int)tr.V0], verticesIndexMap[(int)tr.V1], verticesIndexMap[(int)tr.V2]});
                        faces.Add(f);
                    }
                }
                BRepContainsLineChecker.BRepSolid brepMesh = new BRepContainsLineChecker.BRepSolid(faces);
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
                FacetedBRepSolid brepMesh = new FacetedBRepSolid();

                for (int rengaGridCounter = 0; rengaGridCounter < mesh.GridCount; rengaGridCounter++)
                {
                    Renga.IGrid grid = mesh.GetGrid(rengaGridCounter);

                    Dictionary<int, int> verticesIndexMap = new Dictionary<int, int>();
                    for (int rengaVertexCounter = 0; rengaVertexCounter < grid.VertexCount; rengaVertexCounter++)
                    {
                        Renga.FloatPoint3D p = grid.GetVertex(rengaVertexCounter);
                        verticesIndexMap.Add(rengaVertexCounter, brepMesh.AddPoint(new Point3D(p.X, p.Y, p.Z)));

                    }
                    for (int rengaFaceCounter = 0; rengaFaceCounter < grid.TriangleCount; rengaFaceCounter++)
                    {
                        Renga.Triangle tr = grid.GetTriangle(rengaFaceCounter);
                        Face f = new Face(new int[] { verticesIndexMap[(int)tr.V0], verticesIndexMap[(int)tr.V1], verticesIndexMap[(int)tr.V2] });
                        f.Normal = Face.CalculateNormal(brepMesh.GetPoints(f.Vertices));
                        brepMesh.AddFace(f);
                    }
                }
                brepMesh.CalculateBoundingBox();
                result[rengaMeshCounter] = brepMesh;
            }
            return result;
        }
    }
}

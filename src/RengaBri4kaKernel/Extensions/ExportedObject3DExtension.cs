using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

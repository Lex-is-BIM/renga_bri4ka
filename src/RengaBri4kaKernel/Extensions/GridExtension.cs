using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.Extensions
{
    internal static class GridExtension
    {
        public static Line3D? ExtractExternalContour(this Renga.IGrid rengaGrid)
        {
            Point3D[] tmpPoints = new Point3D[rengaGrid.VertexCount];
            
            for (int vertexIndex = 0; vertexIndex < rengaGrid.VertexCount; vertexIndex++)
            {
                Renga.FloatPoint3D gridVertex = rengaGrid.GetVertex(vertexIndex);
                tmpPoints[vertexIndex] = new Point3D(gridVertex.X, gridVertex.Y, gridVertex.Z);
            }

            double z = tmpPoints[0].Z;

            List<ContourSegment> tmpEdges = new List<ContourSegment>();

            for (int faceIndex = 0; faceIndex < rengaGrid.TriangleCount; faceIndex++)
            {
                Renga.Triangle tr = rengaGrid.GetTriangle(faceIndex);
                Face f = new Face();

                //EdgeTmp e1 = new EdgeTmp(tmpPoints[(int)tr.V0], tmpPoints[(int)tr.V1]);
                //EdgeTmp e2 = new EdgeTmp(tmpPoints[(int)tr.V0], tmpPoints[(int)tr.V2]);
                //EdgeTmp e3 = new EdgeTmp(tmpPoints[(int)tr.V1], tmpPoints[(int)tr.V2]);

                ContourSegment e1 = new ContourSegment(tmpPoints[(int)tr.V0], tmpPoints[(int)tr.V1], z, (int)tr.V0, (int)tr.V1);
                ContourSegment e2 = new ContourSegment(tmpPoints[(int)tr.V0], tmpPoints[(int)tr.V2], z, (int)tr.V0, (int)tr.V2);
                ContourSegment e3 = new ContourSegment(tmpPoints[(int)tr.V1], tmpPoints[(int)tr.V2], z, (int)tr.V1, (int)tr.V2);

                procEdge(e1);
                procEdge(e2);
                procEdge(e3);


                void procEdge(ContourSegment e)
                {
                    if (tmpEdges.Contains(e)) tmpEdges.Remove(e);
                    else tmpEdges.Add(e);
                }
            }

            IsolineGenerator gen = new IsolineGenerator();
            var connectedSegments = gen.ConnectSegmentsIntoPolylines(tmpEdges);

            // Внутреннего контура (count > 1) быть вроде как не может, т.к. солид помещения един
            if (connectedSegments == null || !connectedSegments.Any()) return null;
           

            return new Line3D(connectedSegments.First());
        }
    }
}

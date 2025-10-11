using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RengaBri4kaKernel.Geometry
{
    public enum SolidRelationship
    {
        _Error,
        Separate,      // Solids don't touch or intersect
        Touching,      // Solids touch but don't penetrate
        Intersecting,  // Solids cross each other
        Contains,      // First solid completely contains second
        ContainedBy,   // First solid is completely contained by second
        Equal          // Solids are identical (within tolerance)
    }



    public class FacetedBRepSolid : IGeometryInstance
    {
        //public List<Vector3> Vertices { get; private set; }

        public List<Face> Faces { get; private set; }

        [XmlIgnore]
        public BoundingBox? BBox { get; private set; }

        public FacetedBRepSolid()
        {
            //Vertices = new List<Vector3>();
            Faces = new List<Face>();
        }

        public void AddFace(Face face)
        {
            Faces.Add(face);
        }

        //public List<Vector3> GetPoints(IEnumerable<int> indexes)
        //{
        //    List<Vector3> ps = new List<Vector3>();
        //    foreach (int index in indexes)
        //    {
        //        ps.Add(Vertices[index]);
        //    }
        //    return ps;
        //}

        public override BoundingBox GetBBox()
        {
            if (this.BBox == null) this.BBox = BoundingBox.CalculateFromPoints(this.Faces.SelectMany(f=>f.Vertices).ToArray());
            return this.BBox;
        }

        public override GeometryMode GetGeometryType()
        {
            return GeometryMode.FacetedBRepSolid;
        }



        public void AddFace(List<Vector3> polygon, Vector3 normal)
        {
            if (polygon.Count < 3) return;
            Face faceDef = new Face();

            for (int i = 0; i < polygon.Count; i++)
            {
                faceDef.GetOrAddVertexIndex(polygon[i]);
            }
            faceDef.Normal = normal;

            Faces.Add(faceDef);
        }

        public void AddTriangle(Triangle triangle)
        {
            var indices = new int[3];
            Face faceDef = new Face();
            faceDef.GetOrAddVertexIndex(triangle.V1);
            faceDef.GetOrAddVertexIndex(triangle.V2);
            faceDef.GetOrAddVertexIndex(triangle.V3);
            faceDef.Normal = triangle.CalculateNormal();

            Faces.Add(faceDef);
        }

        public void Merge(FacetedBRepSolid other)
        {
            // Add all faces with adjusted indices
            foreach (var face in other.Faces)
            {
                Faces.Add(face);
            }
        }



        public override string ToString()
        {
            return $"FacetedBrep: {Faces.Count} faces";
        }

    }
}

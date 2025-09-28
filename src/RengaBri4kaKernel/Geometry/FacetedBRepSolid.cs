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

        public override BoundingBox GetBBox()
        {
            if (this.BBox == null) this.BBox = BoundingBox.CalculateFromPoints(this.Points.Select(p => p.Value));
            return this.BBox;
        }

        public void Optimize()
        {
            //TODO: Заменить кучу граней на каноническое описание Faceted BRep (соседние грани в одной плоскости --> плоскость)
        }

        public override GeometryMode GetGeometryType()
        {
            return GeometryMode.FacetedBRepSolid;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public enum GeometryMode
    {
        FacetedBRepSolid,
        Curve3d
    }
    public abstract class IGeometryInstance
    {
        public abstract GeometryMode GetGeometryType();
        public abstract BoundingBox GetBBox();
    }
}

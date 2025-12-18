using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public struct Triangle2
    {
        public Vector3 V1, V2, V3;

        public Triangle2(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        public Vector3 CalculateNormal()
        {
            var edge1 = V2 - V1;
            var edge2 = V3 - V1;
            return Vector3.Cross(edge1, edge2).Normalized();
        }

        public override string ToString()
        {
            return $"Tri[{V1}, {V2}, {V3}]";
        }
    }

}

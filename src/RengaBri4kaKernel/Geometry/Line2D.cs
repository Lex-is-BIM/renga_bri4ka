using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    internal class Line2D
    {
        public Point3D Start;
        public Point3D End;
        public Line2D(Point3D start, Point3D end) { Start = start; End = end; }
    }
}

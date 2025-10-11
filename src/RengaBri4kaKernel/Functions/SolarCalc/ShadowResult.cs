using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.Functions.SolarCalc
{
    public class ShadowResult
    {
        public Point3D ShadowEnd { get; set; }      // End point of shadow on ground
        public double ShadowLength { get; set; }    // Length of shadow
        public double ShadowDirectionX { get; set; } // Shadow direction vector X
        public double ShadowDirectionY { get; set; } // Shadow direction vector Y
        public double ShadowAngle { get; set; }     // Angle of shadow from North (degrees)
    }

}

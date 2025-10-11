using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.Functions.SolarCalc
{
    public class ShadowAnalyzedItem
    {
        public Point3D BasePosition { get; set; }  // Base of the column on ground
        public double Height { get; set; }         // Height of column

        public ShadowAnalyzedItem(double x, double y, double height)
        {
            BasePosition = new Point3D(x, y, 0);
            Height = height;
        }
    }
}

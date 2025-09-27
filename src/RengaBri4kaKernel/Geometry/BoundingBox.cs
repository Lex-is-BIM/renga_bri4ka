using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public class BoundingBox
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public double MinZ { get; set; }
        public double MaxZ { get; set; }

        public double[] GetMinPoint() { return new double[] { MinX, MinY, MinZ };}
        public double[] GetMaxPoint() { return new double[] { MaxX, MaxY, MaxZ }; }
        public double[] GetCentroid()
        {
            return new double[] { (MinX + MaxX) / 2, (MinY + MaxY) / 2, (MinZ + MaxZ) / 2 };
        }

        
        public static BoundingBox GetBBoxFrom(IEnumerable<BoundingBox> bboxes)
        {

            double[] x = new double[bboxes.Count() * 2];
            double[] y = new double[bboxes.Count() * 2];
            double[] z = new double[bboxes.Count() * 2];

            int counter = 0;
            foreach (BoundingBox bbox in bboxes)
            {
                x[counter] = bbox.MinX;
                x[counter] = bbox.MaxX;
                y[counter] = bbox.MinY;
                y[counter] = bbox.MaxY;
                z[counter] = bbox.MinZ;
                z[counter] = bbox.MaxZ;
                counter +=2;
            }

            return new BoundingBox() { MinX = x.Min(), MaxX = x.Max(), MinY = y.Min(),  MaxY = y.Max(), MinZ = z.Min(), MaxZ = z.Max() };
        }
    }
}

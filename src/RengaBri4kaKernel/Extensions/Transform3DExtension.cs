using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{
    public static class Transform3DExtension
    {
        public static double[] GetTransformMatrix(this Renga.ITransform3D transform)
        {
            double[] matrix = new double[16];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i * 4 + j] = transform.GetElement(i, j);
                }   
            }
            return matrix;
        }

        public static void SetTransformMatrix(this Renga.ITransform3D transform, double[] matrix)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    transform.SetElement(i, j, matrix[i * 4 + j]);
                }
            }
        }
    }
}

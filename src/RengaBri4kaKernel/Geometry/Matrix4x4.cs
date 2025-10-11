using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public static class Matrix4x4
    {
        // Matrix element indices (column-major order):
        // [0]  [4]  [8]  [12]
        // [1]  [5]  [9]  [13]
        // [2]  [6]  [10] [14]
        // [3]  [7]  [11] [15]

        public static double[] Identity => new double[]
        {
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
        };

        public static double[] Create(
            double m11, double m12, double m13, double m14,
            double m21, double m22, double m23, double m24,
            double m31, double m32, double m33, double m34,
            double m41, double m42, double m43, double m44)
        {
            return new double[]
            {
            m11, m12, m13, m14,
            m21, m22, m23, m24,
            m31, m32, m33, m34,
            m41, m42, m43, m44
            };
        }

        public static double[] Multiply(double[] a, double[] b)
        {
            if (a.Length != 16 || b.Length != 16)
                throw new ArgumentException("Both matrices must be 4x4 (16 elements)");

            return new double[]
            {
            // Row 0
            a[0] * b[0] + a[4] * b[1] + a[8] * b[2] + a[12] * b[3],
            a[0] * b[4] + a[4] * b[5] + a[8] * b[6] + a[12] * b[7],
            a[0] * b[8] + a[4] * b[9] + a[8] * b[10] + a[12] * b[11],
            a[0] * b[12] + a[4] * b[13] + a[8] * b[14] + a[12] * b[15],
            
            // Row 1
            a[1] * b[0] + a[5] * b[1] + a[9] * b[2] + a[13] * b[3],
            a[1] * b[4] + a[5] * b[5] + a[9] * b[6] + a[13] * b[7],
            a[1] * b[8] + a[5] * b[9] + a[9] * b[10] + a[13] * b[11],
            a[1] * b[12] + a[5] * b[13] + a[9] * b[14] + a[13] * b[15],
            
            // Row 2
            a[2] * b[0] + a[6] * b[1] + a[10] * b[2] + a[14] * b[3],
            a[2] * b[4] + a[6] * b[5] + a[10] * b[6] + a[14] * b[7],
            a[2] * b[8] + a[6] * b[9] + a[10] * b[10] + a[14] * b[11],
            a[2] * b[12] + a[6] * b[13] + a[10] * b[14] + a[14] * b[15],
            
            // Row 3
            a[3] * b[0] + a[7] * b[1] + a[11] * b[2] + a[15] * b[3],
            a[3] * b[4] + a[7] * b[5] + a[11] * b[6] + a[15] * b[7],
            a[3] * b[8] + a[7] * b[9] + a[11] * b[10] + a[15] * b[11],
            a[3] * b[12] + a[7] * b[13] + a[11] * b[14] + a[15] * b[15]
            };
        }

        public static double[] Inverse(double[] matrix)
        {
            if (matrix.Length != 16)
                throw new ArgumentException("Matrix must be 4x4 (16 elements)");

            // Simplified inverse calculation for affine transformation matrices
            // For a complete implementation, you'd want a full matrix inverse

            // Extract the 3x3 rotation/scale part
            double det = matrix[0] * (matrix[5] * matrix[10] - matrix[6] * matrix[9]) -
                        matrix[4] * (matrix[1] * matrix[10] - matrix[2] * matrix[9]) +
                        matrix[8] * (matrix[1] * matrix[6] - matrix[2] * matrix[5]);

            if (Math.Abs(det) < double.Epsilon)
                throw new InvalidOperationException("Matrix is not invertible");

            double invDet = 1.0 / det;

            // Calculate inverse of 3x3 rotation/scale part
            double i11 = (matrix[5] * matrix[10] - matrix[6] * matrix[9]) * invDet;
            double i12 = (matrix[8] * matrix[6] - matrix[4] * matrix[10]) * invDet;
            double i13 = (matrix[4] * matrix[9] - matrix[8] * matrix[5]) * invDet;

            double i21 = (matrix[9] * matrix[2] - matrix[1] * matrix[10]) * invDet;
            double i22 = (matrix[0] * matrix[10] - matrix[8] * matrix[2]) * invDet;
            double i23 = (matrix[8] * matrix[1] - matrix[0] * matrix[9]) * invDet;

            double i31 = (matrix[1] * matrix[6] - matrix[5] * matrix[2]) * invDet;
            double i32 = (matrix[4] * matrix[2] - matrix[0] * matrix[6]) * invDet;
            double i33 = (matrix[0] * matrix[5] - matrix[4] * matrix[1]) * invDet;

            // Calculate inverse translation
            double i41 = -(matrix[12] * i11 + matrix[13] * i21 + matrix[14] * i31);
            double i42 = -(matrix[12] * i12 + matrix[13] * i22 + matrix[14] * i32);
            double i43 = -(matrix[12] * i13 + matrix[13] * i23 + matrix[14] * i33);

            return new double[]
            {
            i11, i12, i13, 0,
            i21, i22, i23, 0,
            i31, i32, i33, 0,
            i41, i42, i43, 1
            };
        }

        public static double[] CreateTranslation(double x, double y, double z)
        {
            return new double[]
            {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            x, y, z, 1
            };
        }

        public static double[] CreateScale(double x, double y, double z)
        {
            return new double[]
            {
            x, 0, 0, 0,
            0, y, 0, 0,
            0, 0, z, 0,
            0, 0, 0, 1
            };
        }

        public static double[] CreateRotationX(double radians)
        {
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            return new double[]
            {
            1, 0, 0, 0,
            0, cos, sin, 0,
            0, -sin, cos, 0,
            0, 0, 0, 1
            };
        }

        public static double[] CreateRotationY(double radians)
        {
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            return new double[]
            {
            cos, 0, -sin, 0,
            0, 1, 0, 0,
            sin, 0, cos, 0,
            0, 0, 0, 1
            };
        }

        public static double[] CreateRotationZ(double radians)
        {
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            return new double[]
            {
            cos, sin, 0, 0,
            -sin, cos, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
            };
        }

        public static string ToString(double[] matrix)
        {
            if (matrix.Length != 16) return "Invalid matrix";

            return $"[{matrix[0]:F2}, {matrix[4]:F2}, {matrix[8]:F2}, {matrix[12]:F2}]\n" +
                   $"[{matrix[1]:F2}, {matrix[5]:F2}, {matrix[9]:F2}, {matrix[13]:F2}]\n" +
                   $"[{matrix[2]:F2}, {matrix[6]:F2}, {matrix[10]:F2}, {matrix[14]:F2}]\n" +
                   $"[{matrix[3]:F2}, {matrix[7]:F2}, {matrix[11]:F2}, {matrix[15]:F2}]";
        }
    }
}

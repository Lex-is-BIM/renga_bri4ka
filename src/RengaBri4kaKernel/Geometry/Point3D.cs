using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    // Basic geometric classures
    public class Point3D
    {
        public double X, Y, Z;
        public Point3D(double x, double y, double z) { X = x; Y = y; Z = z; }
        public Point3D() { X = 0.0; Y = 0.0; Z = 0.0; }
        public static Point3D operator -(Point3D a, Point3D b) => new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Point3D operator +(Point3D a, Point3D b) => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Point3D operator *(Point3D a, double scalar) => new Point3D(a.X * scalar, a.Y * scalar, a.Z * scalar);

        public double Dot(Point3D other) => X * other.X + Y * other.Y + Z * other.Z;
        public Point3D Cross(Point3D other) => new Point3D(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X);
        public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);
        public Point3D Normalize() => this * (1.0 / Length());
    }
}

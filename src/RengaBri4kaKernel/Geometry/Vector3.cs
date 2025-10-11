using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public static class HashCode
    {
        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (value1?.GetHashCode() ?? 0);
                hash = hash * 31 + (value2?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (value1?.GetHashCode() ?? 0);
                hash = hash * 31 + (value2?.GetHashCode() ?? 0);
                hash = hash * 31 + (value3?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (value1?.GetHashCode() ?? 0);
                hash = hash * 31 + (value2?.GetHashCode() ?? 0);
                hash = hash * 31 + (value3?.GetHashCode() ?? 0);
                hash = hash * 31 + (value4?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (value1?.GetHashCode() ?? 0);
                hash = hash * 31 + (value2?.GetHashCode() ?? 0);
                hash = hash * 31 + (value3?.GetHashCode() ?? 0);
                hash = hash * 31 + (value4?.GetHashCode() ?? 0);
                hash = hash * 31 + (value5?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (value1?.GetHashCode() ?? 0);
                hash = hash * 31 + (value2?.GetHashCode() ?? 0);
                hash = hash * 31 + (value3?.GetHashCode() ?? 0);
                hash = hash * 31 + (value4?.GetHashCode() ?? 0);
                hash = hash * 31 + (value5?.GetHashCode() ?? 0);
                hash = hash * 31 + (value6?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (value1?.GetHashCode() ?? 0);
                hash = hash * 31 + (value2?.GetHashCode() ?? 0);
                hash = hash * 31 + (value3?.GetHashCode() ?? 0);
                hash = hash * 31 + (value4?.GetHashCode() ?? 0);
                hash = hash * 31 + (value5?.GetHashCode() ?? 0);
                hash = hash * 31 + (value6?.GetHashCode() ?? 0);
                hash = hash * 31 + (value7?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (value1?.GetHashCode() ?? 0);
                hash = hash * 31 + (value2?.GetHashCode() ?? 0);
                hash = hash * 31 + (value3?.GetHashCode() ?? 0);
                hash = hash * 31 + (value4?.GetHashCode() ?? 0);
                hash = hash * 31 + (value5?.GetHashCode() ?? 0);
                hash = hash * 31 + (value6?.GetHashCode() ?? 0);
                hash = hash * 31 + (value7?.GetHashCode() ?? 0);
                hash = hash * 31 + (value8?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static int Combine(params object[] values)
        {
            if (values == null)
                return 0;

            unchecked
            {
                int hash = 17;
                foreach (var value in values)
                {
                    hash = hash * 31 + (value?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    }

    public struct Vector3
    {
        public double X, Y, Z;

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3 other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !left.Equals(right);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator *(Vector3 a, double scalar) => new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public double Dot(Vector3 other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public Vector3 Normalized()
        {
            double length = Math.Sqrt(LengthSquared());
            if (length < 1e-10) return this;
            return new Vector3(X / length, Y / length, Z / length);
        }

        public override string ToString()
        {
            return $"({X:F3}, {Y:F3}, {Z:F3})";
        }
    }
}

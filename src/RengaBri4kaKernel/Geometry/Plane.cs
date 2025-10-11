using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public struct Plane : IEquatable<Plane>
    {
        public Vector3 Normal { get; }
        public double Distance { get; }
        public Vector3 PointOnPlane { get; }

        public Plane(Vector3 normal, Vector3 point)
        {
            Normal = normal.Normalized();
            PointOnPlane = point;
            Distance = -Normal.Dot(point);
        }

        public Plane(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var edge1 = v2 - v1;
            var edge2 = v3 - v1;
            Normal = Vector3.Cross(edge1, edge2).Normalized();
            PointOnPlane = v1;
            Distance = -Normal.Dot(v1);
        }

        public double DistanceToPoint(Vector3 point)
        {
            return Normal.Dot(point) + Distance;
        }

        public bool IsPointOnPlane(Vector3 point, double tolerance = 1e-10)
        {
            return Math.Abs(DistanceToPoint(point)) < tolerance;
        }

        public bool Equals(Plane other)
        {
            // Planes are equal if they have the same normal and distance (accounting for opposite normals)
            return (VectorEquals(Normal, other.Normal) && Math.Abs(Distance - other.Distance) < 1e-10) ||
                   (VectorEquals(Normal, new Vector3(-other.Normal.X, -other.Normal.Y, -other.Normal.Z)) &&
                    Math.Abs(Distance + other.Distance) < 1e-10);
        }

        public override bool Equals(object obj)
        {
            return obj is Plane other && Equals(other);
        }

        public override int GetHashCode()
        {
            // Use absolute distance for hash code to handle opposite normals
            return HashCode.Combine(
                Math.Abs(Normal.X), Math.Abs(Normal.Y), Math.Abs(Normal.Z),
                Math.Abs(Distance)
            );
        }

        private static bool VectorEquals(Vector3 a, Vector3 b)
        {
            return (a - b).LengthSquared() < 1e-10;
        }
    }

}

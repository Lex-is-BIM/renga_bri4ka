using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Geometry
{
    public struct Edge : IEquatable<Edge>
    {
        public Vector3 Start { get; }
        public Vector3 End { get; }

        public Edge(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }

        public Edge Normalized()
        {
            // Ensure consistent ordering for equality comparison
            if (CompareVectors(Start, End) > 0)
                return new Edge(End, Start);
            return this;
        }

        private static int CompareVectors(Vector3 a, Vector3 b)
        {
            int cmp = a.X.CompareTo(b.X);
            if (cmp != 0) return cmp;
            cmp = a.Y.CompareTo(b.Y);
            if (cmp != 0) return cmp;
            return a.Z.CompareTo(b.Z);
        }

        public bool Equals(Edge other)
        {
            return (VectorEquals(Start, other.Start) && VectorEquals(End, other.End)) ||
                   (VectorEquals(Start, other.End) && VectorEquals(End, other.Start));
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        public override int GetHashCode()
        {
            var normalized = Normalized();
            return HashCode.Combine(
                normalized.Start.GetHashCode(),
                normalized.End.GetHashCode()
            );
        }

        private static bool VectorEquals(Vector3 a, Vector3 b)
        {
            return (a - b).LengthSquared() < 1e-10;
        }

        public override string ToString()
        {
            return $"{Start} -> {End}";
        }
    }

}

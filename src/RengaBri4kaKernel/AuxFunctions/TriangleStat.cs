using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Functions;

namespace RengaBri4kaKernel.AuxFunctions
{
    /// <summary>
    /// Вспомогательный класс для обработки треугольника в пространстве (элемент триангуляции)
    /// </summary>
    public class TriangleStat
    {
        public TriangleStat(double[] p1, double[] p2, double[] p3, SlopeResultUnitsVariant slopeType)
        {
            Point1 = p1;
            Point2 = p2;
            Point3 = p3;
            SlopeType = slopeType;
        }

        public double[] Point1 { get; set; }
        public double[] Point2 { get; set; }
        public double[] Point3 { get; set; }

        public void Calculate(bool useOld = true)
        {
            // Method using distance formula and Heron's formula

            // Calculate side lengths
            double side1 = CalculateDistance(Point1, Point2);
            double side2 = CalculateDistance(Point2, Point3);
            double side3 = CalculateDistance(Point1, Point3);

            // Calculate semi-perimeter
            double s = (side1 + side2 + side3) / 2.0;

            // Calculate area using Heron's formula
            Area = Math.Sqrt(s * (s - side1) * (s - side2) * (s - side3));

            Center = new double[]
            {
                (Point1[0] + Point2[0] + Point3[0])/3.0,
                (Point1[1] + Point2[1] + Point3[1])/3.0,
                (Point1[2] + Point2[2] + Point3[2])/3.0
            };

            
#if DEBUG
            string ncCommand =
                $"point {Point1[0]},{Point1[1]},{Point1[2]}\n" +
                $"point {Point2[0]},{Point2[1]},{Point2[2]}\n" +
                $"point {Point3[0]},{Point3[1]},{Point3[2]}\n";
#endif
            if (!useOld)
            {
                var result = GetPlaneSlope(new List<Vector3>() {
                new Vector3((float)Point1[0], (float)Point1[1], (float)Point1[2]),
                new Vector3((float)Point2[0], (float)Point2[1], (float)Point2[2]),
                new Vector3((float)Point3[0], (float)Point3[1], (float)Point3[2]) });

                Angle = CalculateAngleBetweenVectors(1, 0, result.slopeDirection.X, result.slopeDirection.Y);
                Slope = result.slopeAngle;
            }
            else
            {
                double Slope1, Slope2, Slope3;

                double Angle1, Angle2, Angle3;

                CalcSide(Point1, Point2, out Slope1, out Angle1);
                CalcSide(Point2, Point3, out Slope2, out Angle2);
                CalcSide(Point1, Point3, out Slope3, out Angle3);
                double[] slopesTmp = new double[] { Slope1, Slope2, Slope3 };
                double[] anglesTmp = new double[] { Angle1, Angle2, Angle3 };

                int index = Array.FindIndex(slopesTmp, w => w == slopesTmp.Max());
                Angle = anglesTmp[index];
                Slope = slopesTmp[index];

                // Нужно проверить, имеются ли 2 сходных уклона, и если да -- то угол направить на серединку их противолежащей стороны
                int roundVal = 2;
                if (Math.Round(Slope1, roundVal) == Math.Round(Slope2, roundVal))
                {
                    Angle = CalculateAngleBetweenVectors(1, 0,
                    GetCenter(Point1, Point3)[0] - Point2[0], GetCenter(Point1, Point3)[1] - Point2[1]);
                    //if (Point2[2] > Point1[2]) Angle += Math.PI;
                }
                else if (Math.Round(Slope2, roundVal) == Math.Round(Slope3, roundVal))
                {
                    Angle = CalculateAngleBetweenVectors(1, 0,
                    GetCenter(Point1, Point2)[0] - Point3[0], GetCenter(Point1, Point2)[1] - Point3[1]);
                    //if (Point3[2] > Point1[2]) Angle += Math.PI;
                }
                else if (Math.Round(Slope1, roundVal) == Math.Round(Slope3, roundVal))
                {
                    Angle = CalculateAngleBetweenVectors(1, 0,
                    GetCenter(Point2, Point3)[0] - Point1[0], GetCenter(Point2, Point3)[1] - Point1[1]);
                    //if (Point1[2] > Point2[2]) Angle += Math.PI;
                }
                else
                {
                    //
                }
            }
            

            if (SlopeType == SlopeResultUnitsVariant.Degree | SlopeType == SlopeResultUnitsVariant.Radians)
            {
                Slope = Math.Atan(Slope);
                if (SlopeType == SlopeResultUnitsVariant.Degree) Slope = Slope / Math.PI * 180.0;
            }
            else
            {
                if (SlopeType == SlopeResultUnitsVariant.Promille) Slope *= 1000.0;
                else Slope *= 100.0;
            }

            SlopeStr = Math.Round(Slope, 2).ToString() + " ";
            switch(SlopeType)
            {
                case SlopeResultUnitsVariant.Percent:
                    SlopeStr += "%";
                    break;
                case SlopeResultUnitsVariant.Promille:
                    SlopeStr += "‰";
                    break;
                case SlopeResultUnitsVariant.Degree:
                    SlopeStr += "°";
                    break;

            }
        }

        private double CalculateDistance(double[] p1, double[] p2)
        {
            double dx = p2[0] - p1[0];
            double dy = p2[1] - p1[1];
            double dz = p2[2] - p1[2];
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private void CalcSide(double[] p1, double[] p2, out double Slope, out double Angle)
        {
            double dZ = Math.Abs(p1[2] - p2[2]);
            if (dZ == 0)
            {
                Slope = 0;
                Angle = CalculateAngleBetweenVectors(1, 0, p1[0] - p2[0], p1[1] - p2[1]);
            }
            else
            {
                double[] pMax = p1;
                double[] pMin = p2;

                if (p2[2] > p1[2])
                {
                    pMax = p2;
                    pMin = p1;
                }

                Angle = CalculateAngleBetweenVectors(1, 0, pMin[0] - pMax[0], pMin[1] - pMax[1]);

                double l = CalculateDistance(pMin, new double[] { pMax[0], pMax[1], pMin[2] });
                Slope = dZ / l;
            }
        }

        private double[] GetCenter(double[] p1, double[] p2)
        {
            return new double[]
            {
                (p1[0] + p2[0])/2.0,
                (p1[1] + p2[1])/2.0,
                (p1[2] + p2[2])/2.0
            };
        }


        private double CalculateAngleBetweenVectors(double v1x, double v1y, double v2x, double v2y)
        {
            // Atan2 returns angle in radians between -π and π
            double angleRadians = Math.Atan2(v2y, v2x);

            // Convert to degrees if desired (0° to 360°)
            double angleDegrees = angleRadians * 180.0 / Math.PI;

            // Normalize to 0-360 range
            if (angleDegrees < 0)
                angleDegrees += 360;

            return angleDegrees * Math.PI / 180.0;
        }


        public static (Vector3 normal, Vector3 slopeDirection, double slopeAngle) GetPlaneSlope(List<Vector3> points)
        {
            if (points.Count < 3)
                throw new ArgumentException("At least 3 points are required");

            // Center the points
            Vector3 centroid = GetCentroid(points);
            List<Vector3> centeredPoints = new List<Vector3>();
            foreach (var point in points)
            {
                centeredPoints.Add(point - centroid);
            }

            // Calculate covariance matrix
            double[,] covariance = new double[3, 3];
            foreach (var point in centeredPoints)
            {
                covariance[0, 0] += point.X * point.X;
                covariance[0, 1] += point.X * point.Y;
                covariance[0, 2] += point.X * point.Z;
                covariance[1, 1] += point.Y * point.Y;
                covariance[1, 2] += point.Y * point.Z;
                covariance[2, 2] += point.Z * point.Z;
            }

            covariance[1, 0] = covariance[0, 1];
            covariance[2, 0] = covariance[0, 2];
            covariance[2, 1] = covariance[1, 2];

            // Find eigenvector corresponding to smallest eigenvalue (normal vector)
            Vector3 normal = FindSmallestEigenvector(covariance);
            normal = Vector3.Normalize(normal);

            // Ensure normal points upward (positive Z)
            if (normal.Z < 0) normal = -normal;

            // Calculate slope direction (direction of steepest descent)
            Vector3 slopeDir = new Vector3(-normal.X, -normal.Y, 0);
            if (slopeDir.Length() < 1e-10)
            {
                return (normal, Vector3.Zero, 0);
            }

            slopeDir = Vector3.Normalize(slopeDir);

            // Calculate slope angle in radians
            double slopeAngle = Math.Atan2(Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y),
                                         Math.Abs(normal.Z));

            return (normal, slopeDir, slopeAngle);
        }

        private static Vector3 GetCentroid(List<Vector3> points)
        {
            Vector3 sum = Vector3.Zero;
            foreach (var point in points)
            {
                sum += point;
            }
            return sum / points.Count;
        }

        private static Vector3 FindSmallestEigenvector(double[,] matrix)
        {
            // Simple power iteration for smallest eigenvector
            Vector3 v = new Vector3(1, 1, 1);
            Vector3 vPrev;

            for (int i = 0; i < 100; i++)
            {
                vPrev = v;

                // Apply matrix
                double x = matrix[0, 0] * v.X + matrix[0, 1] * v.Y + matrix[0, 2] * v.Z;
                double y = matrix[1, 0] * v.X + matrix[1, 1] * v.Y + matrix[1, 2] * v.Z;
                double z = matrix[2, 0] * v.X + matrix[2, 1] * v.Y + matrix[2, 2] * v.Z;

                v = new Vector3((float)x, (float)y, (float)z);
                v = Vector3.Normalize(v);

                if (Vector3.Distance(v, vPrev) < 1e-10)
                    break;
            }

            return v;
        }

        public SlopeResultUnitsVariant SlopeType { get; set; }

        #region Calculated Data
        public double[] Center { get; private set; }
        public double Angle { get; private set; }

        public double Area { get; private set; }

        /// <summary>
        /// Уклон в SlopeType
        /// </summary>
        public double Slope { get; private set; }

        public string SlopeStr { get; private set; }
        #endregion

        public override string ToString()
        {
            return
                $"Center={string.Join(",", Center)}\n" +
                $"Angle={Angle} rad; = {Angle / Math.PI * 180.0}°" +
                $"Area={Area} m2\n" +
                $"Slope={Slope} {SlopeType.ToString()}";

        }

    }
}

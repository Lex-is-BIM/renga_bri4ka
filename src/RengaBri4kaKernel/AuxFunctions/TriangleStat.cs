using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Calculate()
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

            //TODO: Переделать расчет как посоветовал Deepseek, не через срединную линию, а просто через рёбра. Так корректнее (наверное). Текущий архивировать
            double Slope1, Slope2, Slope3;

            double Angle1, Angle2, Angle3;

            CalcSide(Point1, Point2, out Slope1, out Angle1);
            CalcSide(Point2, Point3, out Slope2, out Angle2);
            CalcSide(Point1, Point3, out Slope3, out Angle3);
            double[] slopesTmp = new double[] { Slope1, Slope2, Slope3 };
            double[] anglesTmp = new double[] { Angle1, Angle2, Angle3 };
#if DEBUG
            string ncCommand =
                $"point {Point1[0]},{Point1[1]},{Point1[2]}\n" +
                $"point {Point2[0]},{Point2[1]},{Point2[2]}\n" +
                $"point {Point3[0]},{Point3[1]},{Point3[2]}\n";
#endif

            int index = Array.FindIndex(slopesTmp, w => w == slopesTmp.Max());
            Angle = anglesTmp[index];
            Slope = slopesTmp[index];

            // Нужно проверить, имеются ли 2 сходных уклона
            if (Slope1 == Slope2) Angle = CalculateAngleBetweenVectors(1, 0,
                Point2[0] - GetCenter(Point1, Point3)[0], Point2[1] - GetCenter(Point1, Point3)[1]);
            else if (Slope2 == Slope3) Angle = CalculateAngleBetweenVectors(1, 0,
                Point3[0] - GetCenter(Point1, Point2)[0], Point3[1] - GetCenter(Point1, Point2)[1]);
            else if (Slope1 == Slope3) Angle = CalculateAngleBetweenVectors(1, 0,
                Point1[0] - GetCenter(Point3, Point2)[0], Point1[1] - GetCenter(Point3, Point2)[1]);

            Angle += Math.PI;

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

                Angle = CalculateAngleBetweenVectors(1, 0, pMax[0] - pMin[0], pMax[1] - pMin[1]);

                double l = CalculateDistance(pMin, new double[] { pMax[0], pMax[1], pMin[2] });
                Slope = dZ / l;
            }
        }

        private double CalculateDistance(double[] p1, double[] p2)
        {
            double dx = p2[0] - p1[0];
            double dy = p2[1] - p1[1];
            double dz = p2[2] - p1[2];
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
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
            // Alternative method that takes vector components directly
            double dotProduct = v1x * v2x + v1y * v2y;
            double magnitude1 = Math.Sqrt(v1x * v1x + v1y * v1y);
            double magnitude2 = Math.Sqrt(v2x * v2x + v2y * v2y);

            if (magnitude1 == 0 || magnitude2 == 0)
            {
                throw new ArgumentException("Vectors cannot have zero magnitude");
            }

            double cosTheta = dotProduct / (magnitude1 * magnitude2);
            cosTheta = Math.Max(-1.0, Math.Min(1.0, cosTheta));

            return Math.Acos(cosTheta);
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

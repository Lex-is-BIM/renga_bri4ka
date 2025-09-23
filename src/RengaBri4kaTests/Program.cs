using System;

using RengaBri4kaKernel;
using RengaBri4kaKernel.AuxFunctions;
namespace RengaBri4kaTests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TriangleStat tr = new TriangleStat(
                new double[] { 336.1639, 247.3732, 1.2 },
                new double[] { 339.7545, 243.1365, -1 },
                new double[] { 335.6619, 240.0849, 0 },
                SlopeResultUnitsVariant.Degree
                );
            tr.Calculate();

            Console.WriteLine(tr.ToString());

            Console.WriteLine("\nEnd!");
        }
    }
}

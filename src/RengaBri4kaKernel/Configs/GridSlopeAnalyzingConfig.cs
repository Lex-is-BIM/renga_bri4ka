using RengaBri4kaKernel.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Configs
{
    public class GridSlopeAnalyzingConfig : ConfigIO
    {
        public bool IgnoreTrianglesSquareMore { get; set; } = false;
        public double? IgnoringTrianglesSquareMore { get; set; } = 100000;
        public bool IgnoreTrianglesSquareLess { get; set; } = false;
        public double? IgnoringTrianglesSquareLess { get; set; } = 1.0;
        public bool IgnoreValuesMore { get; set; } = false;
        public double? IgnoringValuesMore { get; set; } = 1000;
        public bool IgnoreValuesLess { get; set; } = false;
        public double? IgnoringValuesLess { get; set; } = 10;
        public SlopeResultUnitsVariant Units { get; set; } = SlopeResultUnitsVariant.Promille;
        public bool SaveExtremeResultsToProperties { get; set; } = false;
        public bool AddTriangleAres { get; set; } = false;
        public TextSettingsConfig TextStyle { get; set; } = new TextSettingsConfig();
    }
}

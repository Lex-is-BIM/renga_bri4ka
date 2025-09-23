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
        public double? IgnoringTrianglesSquareMore { get; set; }
        public bool IgnoreTrianglesSquareLess { get; set; } = false;
        public double? IgnoringTrianglesSquareLess { get; set; }
        public bool IgnoreValuesMore { get; set; } = false;
        public double? IgnoringValuesMore { get; set; }
        public bool IgnoreValuesLess { get; set; } = false;
        public double? IgnoringValuesLess { get; set; }
        public SlopeResultUnitsVariant Units { get; set; } = SlopeResultUnitsVariant.Promille;
        public bool CreateNewLevelForResults { get; set; } = true;
        public bool SaveExtremeResultsToProperties { get; set; } = false;
    }
}

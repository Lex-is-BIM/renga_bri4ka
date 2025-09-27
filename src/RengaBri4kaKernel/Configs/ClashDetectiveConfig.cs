using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Configs
{
    public class ClashModes
    {
        public bool Separate { get; set; } = false;
        public bool Touching { get; set; } = true;
        public bool Intersecting { get; set; } = true;
        public bool Contains { get; set; } = true;
        public bool ContainedBy { get; set; } = true;
        public bool Equal { get; set; } = true;
    }
    public class ClashDetectiveConfig : ConfigIO
    {
        public string Name { get; set; }
        public Guid[] Group1 { get; set; }
        public Guid[] Group2 { get; set; }
        public bool AnalyzeBaseLinesOnly { get; set; } = false;
        public ClashModes ClashSettings { get; set; } = new ClashModes();
        public bool CreateReport { get; set; } = false;
        public bool IncludePhotosToReport { get; set; } = false;
    }
}

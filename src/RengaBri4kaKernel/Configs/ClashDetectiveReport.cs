using RengaBri4kaKernel.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Configs
{
    public class ClashDetectiveReportItem
    {
        public string NameObject1 { get; set; }
        public Guid ObjectId1 { get; set; }
        public string CategoryObject1 { get; set; }

        public string NameObject2 { get; set; }
        public Guid ObjectId2 { get; set; }
        public string CategoryObject2 { get; set; }

        public SolidRelationship Relation { get; set; }
        public double[] Centroid { get; set; }

    }
    public class ClashDetectiveReport
    {
        public string DateInfo { get; set; }
        public ClashDetectiveReport()
        {
            DateInfo = DateTime.Now.ToString("G");
            Items = new List<ClashDetectiveReportItem>();
        }
        public List<ClashDetectiveReportItem> Items { get; set; }
    }
}

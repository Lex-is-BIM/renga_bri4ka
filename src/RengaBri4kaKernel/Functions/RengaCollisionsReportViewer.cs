using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Configs;

namespace RengaBri4kaKernel.Functions
{
    internal class RengaCollisionsReportViewer
    {
        public RengaCollisionsReportViewer() { }

        public ClashDetectiveReport? GetLastReport()
        {
            var files = Directory.GetFiles(ClashDetectiveReport.GetSavePath(), "*.xml", SearchOption.TopDirectoryOnly);
            if (!files.Any()) return null;

            FileInfo[] files2 = files.Select(f => new FileInfo(f)).OrderByDescending(f => f.LastWriteTime).ToArray();
            object? result = ConfigIO.LoadFrom<ClashDetectiveReport>(files2.First().FullName);
            return (ClashDetectiveReport?)result;
        }
    }
}

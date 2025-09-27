using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.Configs
{
    public class ClashDetectiveReportItem
    {
        public ClashDetectiveReportItem()
        {
            Date = DateTime.Now.ToString("G");
        }

        public string Date { get; set; }
        public SolidRelationship Relation { get; set; }
        public string NameObject1 { get; set; }
        public Guid ObjectId1 { get; set; }
        public string CategoryObject1 { get; set; }

        public string NameObject2 { get; set; }
        public Guid ObjectId2 { get; set; }
        public string CategoryObject2 { get; set; }

        public double[] BBoxMin { get; set; }
        public double[] BBoxMax { get; set; }

        public override string ToString()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("Дата обнаружения: " + Date);
            report.AppendLine("Тип коллизии: " + Relation.ToString());
            report.AppendLine("Имя первого объекта: " + NameObject1);
            report.AppendLine("Идентификатор первого объекта: " + ObjectId1.ToString("B"));
            report.AppendLine("Категория первого объекта: " + CategoryObject1);
            report.AppendLine("Имя второго объекта: " + NameObject2);
            report.AppendLine("Идентификатор второго объекта: " + ObjectId2.ToString("B"));
            report.AppendLine("Категория второго объекта: " + CategoryObject2);
            //report.AppendLine($"Центроид двух объектов: (X={Centroid[0]}; Y={Centroid[1]}; Z={Centroid[2]})");
            return report.ToString();
        }


    }
    public class ClashDetectiveReport
    {
        public string DateInfo { get; set; }
        public ClashDetectiveReport()
        {
            DateInfo = DateTime.Now.ToString("G");
            Items = new List<ClashDetectiveReportItem>();
        }

        public ClashDetectiveConfig Settings { get; set; }
        public List<ClashDetectiveReportItem> Items { get; set; }

        public static string GetSavePath()
        {
            string path = Path.Combine(PluginConfig.GetDirectoryPath(), "CollisionReports");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }
}

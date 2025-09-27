using RengaBri4kaKernel.AuxFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Configs
{
    public class ClashModes
    {
        //public bool Separate { get; set; } = false;
        public bool Touching { get; set; } = true;
        public bool Intersecting { get; set; } = true;
        public bool Contains { get; set; } = true;
        public bool ContainedBy { get; set; } = true;
        public bool Equal { get; set; } = true;

        public override string ToString()
        {
            StringBuilder report = new StringBuilder();
            //report.AppendLine("Separate: " + Separate.ToString());
            report.AppendLine("Touching: " + Touching.ToString());
            report.AppendLine("Intersecting: " + Intersecting.ToString());
            report.AppendLine("Contains: " + Contains.ToString());
            report.AppendLine("ContainedBy: " + ContainedBy.ToString());
            report.AppendLine("Equal: " + Equal.ToString());
            return report.ToString();
        }
    }
    public class ClashDetectiveConfig : ConfigIO
    {
        public string Name { get; set; }
        public Guid[] Group1 { get; set; }
        public Guid[] Group2 { get; set; }
        public bool AnalyzeBaseLinesOnly { get; set; } = false;
        public bool AddPropertyToObject2By1 { get; set; } = true;
        public ClashModes ClashSettings { get; set; } = new ClashModes();

        /// <summary>
        /// Перечень свойств первых объектов, копируемых вторым, если объекты имеют геометрическую коллизию
        /// </summary>
        public Guid[]? PropertiesToCopy { get; set; }
        public override string ToString()
        {
            StringBuilder report = new StringBuilder();
            var rengaTypes = RengaUtils.GetRengaObjectTypes();

            string[] names1 = Group1.Select(id=> rengaTypes.Where(t=>t.Id == id).First().Name).ToArray();
            string[] names2 = Group2.Select(id => rengaTypes.Where(t => t.Id == id).First().Name).ToArray();

            report.AppendLine(Name);
            report.AppendLine("Объекты из первой группы: " + string.Join(";", names1));
            report.AppendLine("Объекты из второй группы: " + string.Join(";", names2));
            report.Append(ClashSettings.ToString());

            return report.ToString();
        }
    }
}

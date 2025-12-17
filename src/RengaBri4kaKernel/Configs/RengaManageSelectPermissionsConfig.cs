using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RengaBri4kaKernel.Configs
{
    /// <summary>
    /// Параметры настроек отслеживания выбора объектов функции "Запрет выбора"
    /// </summary>
    public class RengaManageSelectPermissionsConfig
    {
        [XmlIgnore]
        public const string NoBehaviourName = "_NO";

        public List<string> PropertyValues { get; set; }
        public List<string> AcceptedRoles { get; set; } //значения из PropertyValues
        public BehaviorOnSelectUnaccessObjects OnSelectMode {  get; set; }

        public RengaManageSelectPermissionsConfig()
        {
            PropertyValues = new List<string>(); //, "Group1", "Group2"
            AcceptedRoles = new List<string>(); //"Group1"
            OnSelectMode = BehaviorOnSelectUnaccessObjects.SkipSelectionWithoutExceptions;
        }
    }
}

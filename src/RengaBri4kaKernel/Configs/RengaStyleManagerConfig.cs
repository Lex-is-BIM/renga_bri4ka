using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.Xml.Linq;

namespace RengaBri4kaKernel.Configs
{
    public enum RengaStyleCategoryVariant
    {
        _Unknown,
        Window,
        Door,
        Element,

        Column,
        Beam,
        Plate,
        Hole,

        Rebar,
        ReinforcementUnit,
        Reinforcement,
        Reinforcment_Connection,

        System,

        PlumbingFixture,
        Equipment,
        Pipe,
        PipeFitting,
        PipeAccessory,

        MechanicalEquipment,
        Duct,
        DuctFitting,
        DuctAccessory,

        WiringAccessory,
        LightingFixture,
        ElectricDistributionBoard,
        ElectricalConductor,
        ElectricalCircuitLine
    }

    public enum StyleTypeVariant
    {
        ObjectInProject,
        STDL
    }


    public abstract class RengaStyleDef
    {
        public RengaStyleDef()
        {
            Name = "";
            Properties = new Dictionary<string, string>();
            StyleCategory = RengaStyleCategoryVariant._Unknown;
        }
        public string Name { get; set; }
        public abstract StyleTypeVariant GetStyleType();
        public RengaStyleCategoryVariant StyleCategory { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Имя файла иконки в подпапке (TODO)
        /// </summary>
        //public string IconPath { get; set; }


        public override bool Equals(object? obj)
        {
            RengaStyleDef? objOther = obj as RengaStyleDef;
            if (objOther == null) return false;

            return
                Name == objOther.Name &&
                StyleCategory == objOther.StyleCategory;
        }
    }

    /// <summary>
    /// Описывает стиль STDL
    /// </summary>
    public class RengaSTDLFile : RengaStyleDef
    {
        public string DefaultName { get; set; } // = Name
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public Version STDL_Version { get; set; } // = version при чтении RST файла

        public override StyleTypeVariant GetStyleType()
        {
            return StyleTypeVariant.STDL;
        }

        public override bool Equals(object? obj)
        {
            RengaSTDLFile? objOther = obj as RengaSTDLFile;
            if (objOther == null) return false;

            return base.Equals(obj) &&
                DefaultName == objOther.DefaultName &&
                Description == objOther.Description &&
                Version == objOther.Version &&
                Author == objOther.Author;
        }
    }


    /// <summary>
    /// Описывает стиль объекта из проекта\шаблона Renga
    /// </summary>
    public class RengaObjectStyleInProject : RengaStyleDef
    {
        public Guid ProjectId { get; set; }
        public Guid ObjectUnuqueId { get; set; }

        public override StyleTypeVariant GetStyleType()
        {
            return StyleTypeVariant.ObjectInProject;
        }

        public override bool Equals(object? obj)
        {
            RengaObjectStyleInProject? objOther = obj as RengaObjectStyleInProject;
            if (objOther == null) return false;

            return
                base.Equals(obj) &&
                ProjectId == objOther.ProjectId &&
                ObjectUnuqueId == objOther.ObjectUnuqueId;
        }

    }

    /// <summary>
    /// Описывает проект Renga с объектами
    /// </summary>
    public class RengaProjectWithObjects
    {
        /// <summary>
        /// Присваивается автоматически при копировании файла во внутренний каталог
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Оригинальное имя файла
        /// </summary>
        public string FileNameOrigin { get; set; } = "";
        public Version ProjectVersion { get; set; } = new Version("1.0.0.0");
    }

    public interface RengaStyleManagerConfigFunc
    {
        public void AddRengaProject(string filePath, bool reSave = false);
        //public void AddObjectInProject(RengaObjectStyleInProject objectDef);
        public bool IsStyleExists(RengaStyleDef style);
        public void AddStdlObject(string stdlPath);

        /// <summary>
        /// Восстанавливает конфиг из кэшированных данных в папке (если конфиг был испорчен)
        /// </summary>
        public void Restore();
    }

    public class RengaStyleTypeDefinition
    {
        public RengaStyleCategoryVariant Bri4kaStyleType { get; set; }
        public string NameRU { get; set; }
        public string NameEN { get; set; }
        public Guid RengaStyleId { get; set; }

        /// <summary>
        /// Категория объекта, которому принадлежит стиль
        /// </summary>
        public Guid RengaObjectType { get; set; }

        /// <summary>
        /// Идентификатор параметра стиля у соответствующего объекта
        /// </summary>
        public Guid RengaStyleParameterId { get; set; }
    }


    public partial class RengaStyleManagerConfig : ConfigIO
    {
        [XmlIgnore]
        public const string RSM_DirectoryName = "RSM";
        [XmlIgnore]
        public const string RSM_ProjectsDirectoryName = "RSM_Projects";

        public static string GetProjectsDirectory()
        {
            string dir = Path.Combine(GetRSMDirectory(), RSM_ProjectsDirectoryName);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        [XmlIgnore]
        public const string RSM_STDLDirectoryName = "RSM_STDL";

        public static string GetStdlDirectory()
        {
            string dir = Path.Combine(GetRSMDirectory(), RSM_STDLDirectoryName);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        [XmlIgnore]
        public const string RSM_ProjectFilePrefix = "RSM_";

        public static string GetRSMDirectory()
        {
            string dir = Path.Combine(PluginConfig.GetDirectoryPath(), "RSM");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }
        public static string GetFilePath()
        {
            return Path.Combine(GetRSMDirectory(), "RengaStyleManagerConfig.xml");
        }
        public List<RengaProjectWithObjects> ProjectsCollection { get; private set; }

        public List<RengaStyleDef> StylesCollecion { get; private set; }

        public static RengaStyleManagerConfig Create()
        {
            if (mInstance == null) 
            {
                mInstance = new RengaStyleManagerConfig();
                
            }
            if (mRengaStylesInfo == null)
            {
                List<RengaStyleTypeDefinition> styles = new List<RengaStyleTypeDefinition>();

                var assembly = Assembly.GetExecutingAssembly();
                string[] res_names = assembly.GetManifestResourceNames();
                IEnumerable<string> res_need = res_names.Where(a => a.Contains("RengaStylesInfo.txt"));
                if (res_need.Any())
                {
                    using (Stream stream = assembly.GetManifestResourceStream(res_need.First()))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string[] fileData = reader.ReadToEnd().Split('\n');
                        foreach (string s in fileData.Skip(1))
                        {
                            string[] styleInfoArr = s.Split(',');
                            Enum.TryParse(styleInfoArr[0], out RengaStyleCategoryVariant styleInternalType);

                            RengaStyleTypeDefinition styleDescr = new RengaStyleTypeDefinition()
                            {
                                Bri4kaStyleType = styleInternalType,
                                RengaStyleId = Guid.Parse(styleInfoArr[1]),
                                RengaObjectType = Guid.Parse(styleInfoArr[2]),
                                RengaStyleParameterId = Guid.Parse(styleInfoArr[3]),
                                NameRU = styleInfoArr[4]
                            };
                            styles.Add(styleDescr);
                        }
                    }
                }
                mRengaStylesInfo = styles.ToArray();
            }
            return mInstance;
        }


        private RengaStyleManagerConfig()
        {
            if (File.Exists(GetFilePath()))
            {
                object? configRaw = ConfigIO.LoadFrom<RengaStyleManagerConfig>(GetFilePath());
                if (configRaw != null)
                {
                    RengaStyleManagerConfig? config = configRaw as RengaStyleManagerConfig;
                    if (config != null)
                    {
                        mInstance = config;
                        return;
                    }
                    else Init();
                }
                else Init();
            }
            else Init();
        }

        private void Init()
        {
            ProjectsCollection = new List<RengaProjectWithObjects>();
            StylesCollecion = new List<RengaStyleDef>();

            if (!Directory.Exists(GetFilePath()))

            // Считать данные из файлов
            this.Restore();
        }

        private static RengaStyleManagerConfig? mInstance;
        private static RengaStyleTypeDefinition[]? mRengaStylesInfo;

        ~RengaStyleManagerConfig()
        {
            ConfigIO.SaveTo(GetFilePath(), this);
        }
    }
}

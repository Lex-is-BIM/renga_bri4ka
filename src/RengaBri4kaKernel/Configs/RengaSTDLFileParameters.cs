using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;


namespace RengaBri4kaKernel.Configs
{

    public class RengaSTDLFileParameters_Metadata
    {
        public string defaultName { get; set; }
        public string description { get; set; }
        public string version { get; set; }
        public string author { get; set; }
    }

    public class RengaSTDLFileParameters_ParamsGroup_Parameter_Item
    {
        public string key { get; set; }
        public string text { get; set; }
    }

    public class RengaSTDLFileParameters_ParamsGroup_Parameter
    {
        public string name { get; set; }
        public string text { get; set; }
        public string type { get; set; }

        public string? entityTypeId { get; set; }

        public string? coreEnumType { get; set; }

        [JsonPropertyName("default")]
        public object? defaultValue { get; set; }

        public double? min { get; set; }
        public double? max { get; set; }

        public List<RengaSTDLFileParameters_ParamsGroup_Parameter_Item>? items { get; set; }
    }
    public class RengaSTDLFileParameters_ParamsGroup
    {
        public string name { get; set; }
        public string text { get; set; }

        [JsonPropertyName("params")]
        public List<RengaSTDLFileParameters_ParamsGroup_Parameter> parameters { get; set; }
    }


    /// <summary>
    /// Описывает JSON-представление параметров RST-файла (созданного с помощью STDL)
    /// </summary>
    public class RengaSTDLFileParameters
    {
        public RengaSTDLFileParameters_Metadata metadata { get; set; }
        public List<RengaSTDLFileParameters_ParamsGroup> styleParameters { get; set; }
        public List<object> ports { get;set; }

        public static RengaSTDLFileParameters? ReadFrom(string fileContent)
        {
            RengaSTDLFileParameters? data = System.Text.Json.JsonSerializer.Deserialize<RengaSTDLFileParameters>(fileContent);
            return data;
        }
    }
}

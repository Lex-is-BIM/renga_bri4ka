using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.Win32;

namespace RengaBri4kaKernel.Configs
{
    /// <summary>
    /// Вспомогательный класс для чтения-записи данных о конфигах функций из/в XML. Также его наследуют все классы конфигураций
    /// </summary>
    public abstract class ConfigIO
    {
        public static object? LoadFrom<ConfigType>(string path)
        {
            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    var serializer = new XmlSerializer(typeof(ConfigType));
                    object? serResult = serializer.Deserialize(stream);
                    return serResult;
                }
            }
            return null;
        }

        public static object? LoadFromWithDialogue<ConfigType>()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбор конфигурационного файла";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Конфиграционный файл (*.XML, *.xml) | *.XML;*.xml";

            openFileDialog.InitialDirectory = Path.Combine(PluginConfig.GetDirectoryPath(), "Configs", typeof(ConfigType).Name);
            if (!Directory.Exists(openFileDialog.InitialDirectory)) Directory.CreateDirectory(openFileDialog.InitialDirectory);

            if (openFileDialog.ShowDialog() == true && File.Exists(openFileDialog.FileName))
            {
                return LoadFrom<ConfigType>(openFileDialog.FileName);
            }
            return null;
        }


        public static void SaveTo<ConfigType>(string path, ConfigType objectData)
        {
            using (var writer = new StreamWriter(path))
            {
                var serializer = new XmlSerializer(typeof(ConfigType));
                serializer.Serialize(writer, objectData);
                writer.Flush();
            }
        }

        public static void SaveToWithDialogue<ConfigType>(ConfigType objectData)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Сохранение файла конфигурации";
            saveFileDialog.Filter = "Конфиграционный файл (*.XML, *.xml) | *.XML;*.xml";
            saveFileDialog.AddExtension = true;

            saveFileDialog.InitialDirectory = Path.Combine(PluginConfig.GetDirectoryPath(), "Configs", typeof(ConfigType).Name);
            if (!Directory.Exists(saveFileDialog.InitialDirectory)) Directory.CreateDirectory(saveFileDialog.InitialDirectory);

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveTo(saveFileDialog.FileName, objectData);
            }
        }

        public static string GetDefaultPath<ConfigType>()
        {
            return Path.Combine(PluginConfig.GetDirectoryPath(), "Configs", typeof(ConfigType).Name, "Default.xml");
        }
    }
}

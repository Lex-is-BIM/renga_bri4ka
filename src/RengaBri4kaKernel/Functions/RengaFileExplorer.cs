using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;
using RengaBri4kaKernel.AuxFunctions;
using Renga;

namespace RengaBri4kaKernel.Functions
{
    /// <summary>
    /// Вспомогательный класс для получения доступа к содержимому файла RNP
    /// </summary>
    public class RengaFileExplorer
    {
        public const string file_checksum = "checksum";
        public const string file_metadata = "metadata";
        public const string tag_AppVersion = "AppVersion";
        public const string tag_SessionId = "SessionId";

        public Dictionary<string, string> RNP_Data;

        public string GetData()
        {
            if (RNP_Data.Any())
            {
                var textData = RNP_Data.Select(d => d.Key + " = " + d.Value);
                return string.Join(Environment.NewLine, textData);
            }
            return "";
        }

        public RengaFileExplorer()
        {
            if (PluginData.Project.HasUnsavedChanges()) PluginData.Project.Save();

            if (!PluginData.Project.HasFile())
            {
                string tmpSavePath = RengaFileExplorer.GenerateTempPath();
                PluginData.Project.SaveAs(tmpSavePath, ProjectType.ProjectType_Project, true);
                startProcessing(tmpSavePath);
            }
            else startProcessing(PluginData.Project.FilePath);
        }
        public RengaFileExplorer(string rnpFilePath)
        {
            startProcessing(rnpFilePath);
        }

        private void startProcessing(string rnpFilePath)
        {
            pRnpFilePath = rnpFilePath;
            RNP_Data = new Dictionary<string, string>();
            ReadData();
        }

        private string GetShortVersion()
        {
            if (RNP_Data.ContainsKey(tag_AppVersion))
            {
                string versionLong = RNP_Data[tag_AppVersion];
                string[] versionArray = versionLong.Split('.');
                return $"{versionArray[0]}.{versionArray[1]}";
            }
            return "";
        }

        public string GetLongVersion()
        {
            if (RNP_Data.ContainsKey(tag_AppVersion)) return RNP_Data[tag_AppVersion];
            return "";
        }

        public static string GenerateTempPath(string ext = ".rnp")
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ext);
        }
        public void ReadData()
        {
            if (File.Exists(pRnpFilePath))
            {
                string copyPath = GenerateTempPath(Path.GetExtension(pRnpFilePath));
                File.Copy(pRnpFilePath, copyPath, true);
                if (!File.Exists(copyPath)) return;
                using (var rnpAsZip = ZipFile.OpenRead(copyPath))
                {
                    foreach (var entry in rnpAsZip.Entries)
                    {
                        if (entry.Name == file_checksum)
                        {
                            var osr = new StreamReader(entry.Open(), Encoding.Default);
                            RNP_Data.Add("checksum", osr.ReadToEnd());
                            osr.Close();
                        }
                        else if (entry.Name == file_metadata)
                        {
                            var osr = new StreamReader(entry.Open(), Encoding.Default);
                            string metadataFile = osr.ReadToEnd();
                            foreach (string str in metadataFile.Split('\n'))
                            {
                                if (str.Contains("="))
                                {
                                    string[] strA = str.Split('=');
                                    RNP_Data[strA[0]] = strA[1];
                                }
                            }
                        }
                    }
                }
                File.Delete(copyPath);
            }
        }

        private string pRnpFilePath;
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Windows;

using RengaBri4kaKernel;
using RengaBri4kaKernel.Functions;
using RengaBri4kaKernel.UI.Windows;
using RengaBri4kaKernel.Configs;

using Renga;


namespace RengaBri4kaLoader
{
    internal enum PluginFunctionVariant
    {
        RENGA_BRI4KA_CALCROOFSLOPES,
        RENGA_BRI4KA_TRIANGLESSTAT,
        RENGA_BRI4KA_FILEMETADATA,
        RENGA_BRI4KA_LEVEL2STAT,
        RENGA_BRI4KA_TEXTCOLORING,
        RENGA_BRI4KA_COLLISIONSMANAGER,
        RENGA_BRI4KA_COLLISIONSVIEWER,
        RENGA_BRI4KA_PLUGINVERSION,
        RENGA_BRI4KA_PLUGINHELP
    }

    internal class PluginMenuItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Tooltip { get; set; } = "";
        public string Category { get; set; }
        public string Version { get; set; }

    }
    internal class PluginFunctions
    {
        public PluginFunctions()
        {
            var Ver = PluginData.rengaApplication.Version;
            PluginData.RengaVersion = new Version(Ver.Major, Ver.Minor, Ver.BuildNumber, Ver.Patch);
        }

        public static PluginFunctions CreateInstance()
        {
            if (mInstance == null) mInstance = new PluginFunctions();

            return mInstance;
        }

        public void Run(PluginMenuItem command)
        {
            if (!Enum.TryParse(command.Id, out PluginFunctionVariant functionVariant)) return;

            //if (PluginData.PluginConfig == null) PluginConfig.Initialize();
            //if (PluginData.PluginConfig == null || PluginData.PluginConfig.RengaVersion == null) return;

            if (PluginData.RengaVersion.CompareTo(new Version(command.Version)) < 0)
            {
                //throw new Exception();
                PluginData.rengaApplication.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Warning, "Сообщение о некорректной версии", $"RengaBri4ka. Функции \"{command.Name}\" требуется версия Renga с " + command.Version);
                return;
            }

            switch (functionVariant)
            {
                //Измерения и статистика
                case PluginFunctionVariant.RENGA_BRI4KA_CALCROOFSLOPES:
                    {
#if DEBUG
                        RengaGridSlopeAnalyzing slopeAnal = new RengaBri4kaKernel.Functions.RengaGridSlopeAnalyzing();
                        slopeAnal.SetInputData(new int[] { 100106, 100107, 100108, 100109}, new GridSlopeAnalyzingConfig() { Units = SlopeResultUnitsVariant.Degree});
                        slopeAnal.Calculate(false);
                        //Bri4ka_CalcGridsSlopes slopesAnalUi = new Bri4ka_CalcGridsSlopes();
                        //slopesAnalUi.ShowDialog();
#else
                        Bri4ka_CalcGridsSlopes slopesAnalUi = new Bri4ka_CalcGridsSlopes();
                        slopesAnalUi.ShowDialog();       
#endif

                    }

                    break;
                case PluginFunctionVariant.RENGA_BRI4KA_TRIANGLESSTAT:
                    RengaGeometryStat stat = new RengaGeometryStat();
                    stat.Calculate();
                    break;
                case PluginFunctionVariant.RENGA_BRI4KA_FILEMETADATA:
                    {
                        if (PluginData.Project.HasUnsavedChanges()) PluginData.Project.Save();
                        RengaFileExplorer rnpExplorer = new RengaFileExplorer(PluginData.Project.FilePath);

                        Bri4ka_TextForm.ShowTextWindow(rnpExplorer.GetData(), "Параметры проекта Renga");
                        break;
                    }

                // Оформление
                case PluginFunctionVariant.RENGA_BRI4KA_TEXTCOLORING:
                    Bri4ka_ColorPallette textColoringUi = new Bri4ka_ColorPallette(ColoringFunctionMode.ColorSelectedText);
                    textColoringUi.ShowDialog();
                    break;

                //Утилиты
                case PluginFunctionVariant.RENGA_BRI4KA_COLLISIONSMANAGER:
                    Bri4ka_ClashDetective collisionUI = new Bri4ka_ClashDetective();
                    collisionUI.ShowDialog();
                    break;
                case PluginFunctionVariant.RENGA_BRI4KA_COLLISIONSVIEWER:
                    Bri4ka_CollisionsReportViewer collisionViewerUI = new Bri4ka_CollisionsReportViewer();
                    //System.Windows.Application.Current.Run(collisionViewerUI);
                    collisionViewerUI.Show();
                    break;

                //Настройки
                case PluginFunctionVariant.RENGA_BRI4KA_PLUGINVERSION:
                    {
                        var ass_info = Assembly.GetExecutingAssembly().GetName();
                        MessageBox.Show("Версия плагина: " + ass_info.Version.ToString());
                    }
                    break;
                case PluginFunctionVariant.RENGA_BRI4KA_PLUGINHELP:
                    {
                        string pdfGuidePath = Path.Combine(PluginData.PluginFolder, "Bri4kaGuide.pdf");
                        if (File.Exists(pdfGuidePath))
                        {
                            using Process fileopener = new Process();
                            fileopener.StartInfo.FileName = "explorer";
                            fileopener.StartInfo.Arguments = "\"" + pdfGuidePath + "\"";

                            fileopener.Start();
                            //Process.Start("explorer.exe", pdfGuidePath);
                        }
                        
                    }
                    break;

            }
        }

        private void InitVersion()
        {

        }
        private static PluginFunctions? mInstance;

    }
}

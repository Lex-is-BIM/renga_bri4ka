using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Interop;

using RengaBri4kaKernel;
using RengaBri4kaKernel.Configs;

using Renga;

namespace RengaBri4kaLoader
{
    public class PluginLoader : IPlugin
    {
        private Renga.IApplication m_app;
        private readonly List<Renga.ActionEventSource> m_eventSources = new List<Renga.ActionEventSource>();

        public bool Initialize(string pluginFolder)
        {
            PluginData.PluginFolder = pluginFolder;
            Renga.Application rengaApp = new Renga.Application();
            m_app = rengaApp;

            PluginData.rengaApplication = rengaApp;
            PluginData.IsRengaProfeccional = rengaApp.ProductName.Contains("Renga Professional");

            Renga.IUI rengaUI = rengaApp.UI;
            InitMenu(pluginFolder, rengaUI);
            return true;
        }

        private void InitMenu(string pluginDir, Renga.IUI rengaUI)
        {
            Renga.IUIPanelExtension pluginPanel = rengaUI.CreateUIPanelExtension();
            Renga.IDropDownButton pluginButton = rengaUI.CreateDropDownButton();

            var PluginIconDef = GetIcon(rengaUI, "RENGA_BRI4KA_MAIN_32x32");
            if (PluginIconDef != null) pluginButton.Icon = PluginIconDef;

            string PluginMenuFile = Path.Combine(pluginDir, "PluginMenu.tsv");
            Dictionary<string, List<PluginMenuItem>> CategorizedFunctions = new Dictionary<string, List<PluginMenuItem>>();
            if (!File.Exists(PluginMenuFile)) throw new FileNotFoundException("RengaBri4ka. Путь к файлу с данными о командах не найден!");
            foreach (string commandInfoStr in File.ReadAllLines(PluginMenuFile, System.Text.Encoding.UTF8).Skip(1))
            {
                string[] commandInfoArray = commandInfoStr.Split('\t');
                PluginMenuItem commandInfo = new PluginMenuItem()
                {
                    Id = commandInfoArray[0],
                    Name = commandInfoArray[1],
                    Tooltip = commandInfoArray[2],
                    Category = commandInfoArray[3],
                    Version = commandInfoArray[4]
                };
                if (!CategorizedFunctions.ContainsKey(commandInfo.Category)) CategorizedFunctions.Add(commandInfo.Category, new List<PluginMenuItem>());
                CategorizedFunctions[commandInfo.Category].Add(commandInfo);
            }

            //Создание меню. Категории будут SPlitButton, функции категории обычными ToolTip button
            foreach (var categoryFunctionCollection in CategorizedFunctions)
            {
                pluginButton.AddSeparator();
                Renga.IAction functionsCategory = rengaUI.CreateAction();
                functionsCategory.DisplayName = categoryFunctionCollection.Key;
                var CategoryIconDef = GetIcon(rengaUI, "ICON_CAT_" + categoryFunctionCollection.Key);
                if (CategoryIconDef != null) functionsCategory.Icon = CategoryIconDef;
                functionsCategory.Enabled = false;
                pluginButton.AddAction(functionsCategory);

                foreach (var menuItem in categoryFunctionCollection.Value)
                {
                    Renga.IAction menuAction = rengaUI.CreateAction();
                    menuAction.DisplayName = menuItem.Name;
                    menuAction.ToolTip = menuItem.Tooltip;

                    var iconDef = GetIcon(rengaUI, menuItem.Id);
                    if (iconDef != null) menuAction.Icon = iconDef;

                    ActionEventSource functionEvent = new ActionEventSource(menuAction);
                    functionEvent.Triggered += (o, s) =>
                    {
                        PluginFunctions.CreateInstance().Run(menuItem);
                    };
                    m_eventSources.Add(functionEvent);

                    pluginButton.AddAction(menuAction);
                }
            }

            pluginPanel.AddDropDownButton(pluginButton);
            rengaUI.AddExtensionToPrimaryPanel(pluginPanel);
        }

        private Renga.IImage? GetIcon(Renga.IUI rengaUI, string? iconName)
        {
            if (iconName == null) return null;
            Renga.IImage iconDef = rengaUI.CreateImage();

            string iconFilePath = Path.Combine(PluginData.PluginFolder, "icons", iconName + ".png");
            if (File.Exists(iconFilePath))
            {
                iconDef.LoadFromFile(iconFilePath);
                return iconDef;
            }
            return null;
        }
        public void Stop()
        {
            foreach (var eventSource in m_eventSources)
            {
                eventSource.Dispose();
            }
            m_eventSources.Clear();

            //if (PluginData.PluginConfig != null) ConfigIO.SaveTo<PluginConfig>(PluginConfig.GetConfigPath(), PluginData.PluginConfig);
        }
    }
}

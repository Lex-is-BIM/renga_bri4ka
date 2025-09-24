using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RengaBri4kaKernel.UI
{
    internal interface IWindowWithConfig<ConfigType>
    {
        public ConfigType GetConfigFromUI();
        public void SetConfigToUi(ConfigType? config);

        public void Button_SaveSettingsToFile_Click(object sender, RoutedEventArgs e);
        public void Button_LoadSettingsFromFile_Click(object sender, RoutedEventArgs e);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.AuxFunctions
{
    /// <summary>
    /// Вспомогательный класс, осуществлящий ввод действий со стороны Пользователя
    /// </summary>
    internal class UserInput
    {
        public static int[]? GetSelectedObjects()
        {
            Array objects = PluginData.rengaApplication.Selection.GetSelectedObjects();
            if (objects.Length > 0) return objects.Cast<int>().ToArray();
            return null;
        }
    }
}

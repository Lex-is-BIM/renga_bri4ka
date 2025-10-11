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
        public static IEnumerable<Renga.IModelObject>? GetModelObjectsByTypes(Guid[] types)
        {
            if (PluginData.Project == null) return null;

            List<Renga.IModelObject> result = new List<Renga.IModelObject>();
            Renga.IModel model = PluginData.Project.Model;
            Renga.IModelObjectCollection objects = model.GetObjects();
            for (int i = 0; i < objects.Count; i++)
            {
                Renga.IModelObject o = objects.GetByIndex(i);
                if (types.Contains(o.ObjectType)) result.Add(o);
            }
            return result;
        }
    }
}

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
        public static IEnumerable<Renga.IModelObject>? GetModelObjectsByTypes(Guid[] types, bool onlyVisible = false)
        {
            if (PluginData.Project == null) return null;

            var view = PluginData.rengaApplication.ActiveView;
            var modelView = view as Renga.IModelView;

            List<Renga.IModelObject> result = new List<Renga.IModelObject>();
            Renga.IModel model = PluginData.Project.Model;
            Renga.IModelObjectCollection objects = model.GetObjects();
            for (int i = 0; i < objects.Count; i++)
            {
                Renga.IModelObject o = objects.GetByIndex(i);
                
                if (types.Contains(o.ObjectType)) 
                {
                    if (onlyVisible && modelView != null && modelView.IsObjectVisible(o.Id)) result.Add(o);
                    else if (!onlyVisible) result.Add(o);
                }
            }
            return result;
        }
    }
}

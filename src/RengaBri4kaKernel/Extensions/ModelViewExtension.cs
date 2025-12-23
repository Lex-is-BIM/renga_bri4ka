using Renga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{
    public enum ObjectsVisibilityVariant
    {
        ShowAll,
        HideAll,
        ShowOnlySelected,
        HideOnlySelected
    }
    public static class ModelViewExtension
    {
        public static void SetObjectsVisibility2(this Renga.IModelView rengaModelView, ObjectsVisibilityVariant mode, int[]? ids)
        {
            List<int> idsAll = new List<int>();
            Renga.IModel model = PluginData.Project.Model;
            Renga.IModelObjectCollection rengaObjectsCollection = model.GetObjects();

            for (int rengaObjectIndex = 0; rengaObjectIndex < rengaObjectsCollection.Count; rengaObjectIndex++)
            {
                Renga.IModelObject rengaObject = rengaObjectsCollection.GetByIndex(rengaObjectIndex);
                idsAll.Add(rengaObject.Id);
            }

            if (mode == ObjectsVisibilityVariant.ShowAll) rengaModelView.SetObjectsVisibility(idsAll.ToArray(), true);
            else if (mode == ObjectsVisibilityVariant.HideAll) rengaModelView.SetObjectsVisibility(idsAll.ToArray(), false);
            else if (ids == null || !ids.Any()) return;


            if (mode == ObjectsVisibilityVariant.ShowOnlySelected)
            {
                var idsToHide = idsAll.Except(ids);

                rengaModelView.SetObjectsVisibility(ids, true);
                if (idsToHide.Any()) rengaModelView.SetObjectsVisibility(idsToHide.ToArray(), false);
            }
            else if (mode == ObjectsVisibilityVariant.HideOnlySelected)
            {
                var idsToShow = idsAll.Except(ids);

                rengaModelView.SetObjectsVisibility(ids, false);
                if (idsToShow.Any()) rengaModelView.SetObjectsVisibility(idsToShow.ToArray(), true);
            }
        }
    }
}

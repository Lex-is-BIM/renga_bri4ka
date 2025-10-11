using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{
    public static class SelectionExtension
    {
        public static IEnumerable<Renga.IModelObject>? GetSelectedObjects2(this Renga.ISelection selection, bool onlyWithGeometry3d = false)
        {
            Array objects = PluginData.rengaApplication.Selection.GetSelectedObjects();
            if (objects.Length == 0) return null;
            int[] objectsIds = objects.Cast<int>().ToArray();

            Renga.IModel rengaModel = PluginData.Project.Model;
            Renga.IModelObjectCollection rengaModelObjects = rengaModel.GetObjects();
            List<Renga.IModelObject> needObjects = new List<Renga.IModelObject>();
            foreach (int objectId in objectsIds)
            {
                Renga.IModelObject rengaObject = rengaModelObjects.GetById(objectId);
                if (onlyWithGeometry3d && rengaObject.GetExportedObject3D() != null) needObjects.Add(rengaObject);
                else if (!onlyWithGeometry3d) needObjects.Add(rengaObject);
            }

            return needObjects;
        }
    }
}

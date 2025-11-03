using RengaBri4kaKernel.Functions;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.AuxFunctions
{
    internal class RengaUtils
    {
        public static RengaTypeInfo[] GetRengaObjectTypes()
        {
            var ids = typeof(Renga.ObjectTypes).GetRuntimeFields();
            RengaTypeInfo[] rengaTypesInfo = new RengaTypeInfo[ids.Count()];
            for (int i = 0; i < ids.Count(); i++)
            {
                FieldInfo field = ids.ElementAt(i);
                var startIdx = field.Name.IndexOf('<');
                var endIdx = field.Name.IndexOf('>');
                rengaTypesInfo[i] = new RengaTypeInfo() { Id = (Guid)field.GetValue(null)!, Name = field.Name.Substring(startIdx + 1, endIdx - 1) };
            }
            return rengaTypesInfo;
        }

        public static int[]? ConvertUniqueIdsToId(Guid[]? objectsUniqId)
        {
            if (PluginData.Project == null) return null;
            if (objectsUniqId == null) return null;
            List<int> ids = new List<int>();

            Renga.IModel model = PluginData.Project.Model;
            Renga.IModelObjectCollection objects = model.GetObjects();

            for (int i = 0; i < objects.Count; i++)
            {
                Renga.IModelObject o = objects.GetByIndex(i);
                if (objectsUniqId.Contains(o.UniqueId)) ids.Add(o.Id);
            }
            return ids.ToArray();
        }

        public static void EditVisibility(int[]? BesideIt)
        {
            var view = PluginData.rengaApplication.ActiveView;
            var modelView = view as Renga.IModelView;
            if (modelView == null) return;

            List<int> idToHide = new List<int>();
            List<int> idToShow = new List<int>();
            Renga.IModel model = PluginData.Project.Model;
            Renga.IModelObjectCollection objects = model.GetObjects();

            for (int i = 0; i < objects.Count; i++)
            {
                Renga.IModelObject o = objects.GetByIndex(i);
                idToHide.Add(o.Id);

                if (BesideIt != null && BesideIt.Contains(o.Id)) idToShow.Add(o.Id);
            }
            modelView.SetObjectsVisibility(idToHide.ToArray(), false);
            if (idToShow.Any()) modelView.SetObjectsVisibility(idToShow.ToArray(), true);
        }

        public static void SetObjectsSelected(int[]? objectsUniqIdToSelect)
        {
            if (objectsUniqIdToSelect == null) return;

            Renga.IModel model = PluginData.Project.Model;
            PluginData.rengaApplication.Selection.SetSelectedObjects(objectsUniqIdToSelect);
        }
        public static void SetObjectsSelected(Guid[]? objectsUniqIdToSelect)
        {
            if (objectsUniqIdToSelect == null) return;
            int[]? ids = ConvertUniqueIdsToId(objectsUniqIdToSelect);
            SetObjectsSelected(ids);
        }

        public static void LookTo(double[] pos, double[] focusPoint, double[] upVector)
        {
            Renga.IView view = PluginData.rengaApplication.ActiveView as Renga.IView;
            if (view.Type != Renga.ViewType.ViewType_View3D) return;
            Renga.IModelView? viewModel = view as Renga.IModelView;
            if (viewModel == null) return;
            Renga.IView3DParams? viewModelParams = viewModel as Renga.IView3DParams;
            if (viewModelParams == null) return;

            Renga.ICamera3D camera = viewModelParams.Camera;
            camera.LookAt(
                new Renga.FloatPoint3D() { X = (float)focusPoint[0], Y = (float)focusPoint[1], Z = (float)focusPoint[2] },
                new Renga.FloatPoint3D() { X = (float)pos[0], Y = (float)pos[1], Z = (float)pos[2] },
                new Renga.FloatVector3D() { X = (float)upVector[0], Y = (float)upVector[1], Z = (float)upVector[2] });
        }

        public static void ShowMessageBox(string text, bool prefix = true)
        {
            string prefixStr = "Bri4ka ";
            if (!prefix) prefixStr = "";

            string mess = prefixStr + text;
            PluginData.rengaApplication.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Warning, "Bri4ka Предупреждение", text);
        }

    }

    internal class RengaTypeInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}

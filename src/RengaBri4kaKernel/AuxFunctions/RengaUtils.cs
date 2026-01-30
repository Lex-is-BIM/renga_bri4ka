using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Functions;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.AuxFunctions
{
    internal class RengaUtils
    {

        //public static RengaTypeInfo[] GetRengaObjectTypes(bool only3d = false)
        //{
        //    var objects_3d = RengaObjectTypes.GetObject3dCategories();

        //    if (mRengaEntTypes == null)
        //    {
        //        var ids = typeof(Renga.EntityTypes).GetRuntimeFields();
        //        mRengaEntTypes = new RengaTypeInfo[ids.Count()];
        //        for (int i = 0; i < ids.Count(); i++)
        //        {
        //            FieldInfo field = ids.ElementAt(i);
        //            var startIdx = field.Name.IndexOf('<');
        //            var endIdx = field.Name.IndexOf('>');
        //            mRengaEntTypes[i] = new RengaTypeInfo() { Id = (Guid)field.GetValue(null)!, Name = field.Name.Substring(startIdx + 1, endIdx - 1) };
        //        }
        //    }
        //    return mRengaEntTypes.OrderBy(t=>t.Name).ToArray();
        //}
        private static RengaTypeInfo[]? mRengaEntTypes;

        public static int[]? ConvertUniqueIdsToId(Guid[]? objectsUniqId)
        {
            if (PluginData.Project == null) return null;
            if (objectsUniqId == null) return null;
            List<int> ids = new List<int>();

            Renga.IModel model = PluginData.Project.Model;
            Renga.IModelObjectCollection? objects = model.GetObjects();

            for (int i = 0; i < objects.Count; i++)
            {
                Renga.IModelObject o = objects.GetByIndex(i);
                if (objectsUniqId.Contains(o.UniqueId)) ids.Add(o.Id);
            }
            return ids.ToArray();
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

        public static void ShowMessageBox(string text, bool prefix = true)
        {
            string prefixStr = "Bri4ka ";
            if (!prefix) prefixStr = "";

            string mess = prefixStr + text;
            PluginData.rengaApplication.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Warning, "Bri4ka Предупреждение", text);
        }

        public static void AddLog(string text)
        {

#if DEBUG
            Logger.Write(text);
#endif
        }

    }

    internal class Logger
    {
        private Logger()
        {
            if (File.Exists(pLogPath)) File.Delete(pLogPath); 
        }
        public static void Write(string text)
        {
            if (mInstance == null) mInstance = new Logger();

            File.AppendAllText(pLogPath, text);
        }


        private static string pLogPath = "tmpLog.txt";
        private static Logger? mInstance;
    }

    internal class RengaTypeInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}

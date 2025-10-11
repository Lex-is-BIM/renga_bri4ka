using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Geometry;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Functions
{
    /// <summary>
    /// Создание перекрытий по помещениям
    /// </summary>
    public class RengaFloorByRoomsCreator
    {
        public RengaFloorByRoomsCreator()
        {

        }

        public void Start()
        {

            IEnumerable<Renga.IModelObject>? selectedObjects = PluginData.rengaApplication.Selection.GetSelectedObjects2(true);
#if DEBUG
            //selectedObjects = UserInput.GetModelObjectsByTypes(new Guid[] { RengaObjectTypes.Room });
#endif
            if (selectedObjects == null)
            {
                PluginData.rengaApplication.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Warning, "Сообщение", $"RengaBri4ka. Объекты не были выбраны!");
                return;
            }
            List<Renga.IModelObject> selectedObjectsRooms = new List<Renga.IModelObject>();
            foreach (var selectedObject in selectedObjects)
            {
                if (selectedObject.ObjectType == RengaObjectTypes.Room) selectedObjectsRooms.Add(selectedObject);
            }

            if (!selectedObjectsRooms.Any())
            {
                PluginData.rengaApplication.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Warning, "Сообщение", $"RengaBri4ka. Среди выбранных объектов отсутствуют помещения! Выполнение функции будет прервано!");
                return;
            }

            foreach (var selectedObject in selectedObjectsRooms)
            {
                var lineGeometry = selectedObject.GetExternalBorder((int)Renga.GridTypes.Room.Floor);
                if (lineGeometry == null) continue;

                List<Point3D> points = lineGeometry.Vertices.Select(p => new Point3D(p.X, p.Y, p.Z)).ToList();

                Renga.IModelObject? createdObject = PluginData.Project.Model.CreateBaselineObject(BaselineObjectType.Floor, points, false);
            }

        }
    }
}

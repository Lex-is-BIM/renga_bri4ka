using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Functions
{
    public enum ObjectPinFunctionVariant
    {
        PineLinked,
        UnPinAll,
        PinAll
    }

    //Работа с блокировками объектов
    public class RengaObjectPinFunctions
    {
        public static void setPinned(bool pinStatus, Guid[]? categories)
        {
            Renga.IProject? rengaProject = PluginData.Project;
            if (rengaProject == null) return;

            Renga.IModel rengaModel = rengaProject.Model;
            if (rengaModel == null) return;


            Renga.IModelObject[]? rengaObjects = rengaModel.GetObjects2(categories);
            if (rengaObjects == null || !rengaObjects.Any()) return;

            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            foreach (Renga.IModelObject rengaObject in rengaObjects)
            {
                rengaObject.Pinned = pinStatus;
            }
            editOperation.Apply();
        }

        public static void pinLinked()
        {
            Renga.IProject? rengaProject = PluginData.Project;
            if (rengaProject == null) return;

            Renga.IModel rengaModel = rengaProject.Model;
            if (rengaModel == null) return;



            //---Уровни
            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            Renga.IModelObject[]? rengaLevels = rengaModel.GetObjects2(new Guid[] { RengaObjectTypes.Level });
            if (rengaLevels != null && rengaLevels.Any())
            {
                foreach (Renga.IModelObject rengaObject in rengaLevels)
                {
                    Renga.IModelObject[]? levelObjects = rengaModel.GetObjectsOnLevel(rengaObject.Id);
                    setObjectsPinStatus(levelObjects, rengaObject.Pinned);
                }
            }
           editOperation.Apply();

            //---Трассы и оборудование
            editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            Renga.IModelObject[]? rengaRoutes = rengaModel.GetObjects2(new Guid[] { RengaObjectTypes.Route });
            if (rengaRoutes != null && rengaRoutes.Any())
            {
                foreach (Renga.IModelObject rengaObject in rengaRoutes)
                {
                    Renga.IRouteParams? rengaObjectRoute = rengaObject.GetInterfaceByName("IRouteParams") as Renga.IRouteParams;
                    if (rengaObjectRoute == null) continue;

                    int[] dependedIds = new int[rengaObjectRoute.GetObjectOnRouteCount()];
                    for (int objOnRouteIndex = 0; objOnRouteIndex < rengaObjectRoute.GetObjectOnRouteCount(); objOnRouteIndex++)
                    {
                        dependedIds[objOnRouteIndex] = rengaObjectRoute.GetObjectOnRoutePlacement(objOnRouteIndex).Id;
                    }

                    if (dependedIds.Length < 1) continue;

                    Renga.IModelObject[] dependedObjects = rengaModel.GetObjectsByIntIds(dependedIds);
                    setObjectsPinStatus(dependedObjects, rengaObject.Pinned);
                }
            }

            editOperation.Apply();
            //--- Стены и перекрытия
            editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();
            

            Renga.IModelObject[]? rengaGroup1 = rengaModel.GetObjects2(new Guid[] { RengaObjectTypes.Floor, RengaObjectTypes.Wall });
            if (rengaGroup1 != null && rengaGroup1.Any())
            {
                foreach (Renga.IModelObject rengaObject in rengaGroup1)
                {
                    Array dependObjectIds = new int[] { };
                    if (rengaObject.ObjectType == RengaObjectTypes.Floor)
                    {
                        Renga.IFloorParams? rengaObjectFloor = rengaObject.GetInterfaceByName("IFloorParams") as Renga.IFloorParams;
                        if (rengaObjectFloor != null) dependObjectIds = rengaObjectFloor.GetDependentObjectIds();
                    }
                    else if (rengaObject.ObjectType == RengaObjectTypes.Wall)
                    {
                        Renga.IWallParams? rengaObjectWall = rengaObject.GetInterfaceByName("IWallParams") as Renga.IWallParams;
                        if (rengaObjectWall != null) dependObjectIds = rengaObjectWall.GetDependentObjectIds();
                    }

                    if (dependObjectIds.Length < 1) continue;

                    int[] dependedIds = dependObjectIds.Cast<int>().ToArray();
                    Renga.IModelObject[] dependedObjects = rengaModel.GetObjectsByIntIds(dependedIds);

                    setObjectsPinStatus(dependedObjects, rengaObject.Pinned);
                }
            }
            editOperation.Apply();



        }

        private static void setObjectsPinStatus(Renga.IModelObject[]? rengaObjects, bool status)
        {
            if (rengaObjects == null || !rengaObjects.Any()) return;

            foreach (Renga.IModelObject rengaObject in rengaObjects)
            {
                rengaObject.Pinned = status;
            }
        }
    }
}

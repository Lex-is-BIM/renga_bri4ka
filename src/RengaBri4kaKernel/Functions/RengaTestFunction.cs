
namespace RengaBri4kaKernel.Functions
{
    static class CubeExtension
    {
        public static Renga.Point3D GetMinPointMeters(this Renga.Cube rengaCube)
        {
            return new Renga.Point3D() { X = rengaCube.MIN.X / 1000.0, Y = rengaCube.MIN.Y / 1000.0, Z = rengaCube.MIN.Z / 1000.0 };
        }

        public static Renga.Point3D GetMaxPointMeters(this Renga.Cube rengaCube)
        {
            return new Renga.Point3D() { X = rengaCube.MAX.X / 1000.0, Y = rengaCube.MAX.Y / 1000.0, Z = rengaCube.MAX.Z / 1000.0 };
        }

        public static double GetArea(this Renga.Cube rengaCube)
        {
            var minP = rengaCube.GetMinPointMeters();
            var maxP = rengaCube.GetMaxPointMeters();
            return (maxP.X - minP.X) * (maxP.Y - minP.Y);
        }

        public static double GetVolume(this Renga.Cube rengaCube)
        {
            var minP = rengaCube.GetMinPointMeters();
            var maxP = rengaCube.GetMaxPointMeters();

            return (maxP.X - minP.X) * (maxP.Y - minP.Y) * (maxP.Z - minP.Z);
        }

        public static string GetMinPointMetersStr(this Renga.Cube rengaCube)
        {
            return getPointInMetersStr(rengaCube.GetMinPointMeters());
        }

        public static string GetMaxPointMetersStr(this Renga.Cube rengaCube)
        {
            return getPointInMetersStr(rengaCube.GetMaxPointMeters());
        }

        private static string getPointInMetersStr(Renga.Point3D rengaPoint)
        {
            return $"{(rengaPoint.X).ToString("0.00")};{(rengaPoint.Y).ToString("0.00")};{(rengaPoint.Z).ToString("0.00")}";
        }
    }

    public class RengaTestFunction
    {

        public static void Start()
        {
            Renga.Application rengaApplication = new Renga.Application();
            Renga.IProject rengaProject = rengaApplication.Project;

            Renga.IModel rengaModel = rengaProject.Model;

            // Работа с моделью
            Renga.Cube modelBboxInfo = rengaModel.GetBoundingBox();

            var editOperation = rengaApplication.Project.CreateOperation();
            editOperation.Start();

            string modelBboxInfoText = "" +
                $"Минимальная точка, метры = {modelBboxInfo.GetMinPointMetersStr()}\n" +
                $"Максимальная точка, метры = {modelBboxInfo.GetMaxPointMetersStr()}\n" +
                $"Площадь = {modelBboxInfo.GetArea().ToString("0.000")} м²\n" +
                $"Объём = {modelBboxInfo.GetVolume().ToString("0.000")} м³";

            // Анализ сборок

            // Выборка сборок из модели
            Renga.IModelObjectCollection? allObjects = rengaModel.GetObjects();
            if (allObjects == null) return;

            System.Collections.Generic.List<Renga.IModelObject> resultObjects = new System.Collections.Generic.List<Renga.IModelObject>();
            for (int i = 0; i < allObjects.Count; i++)
            {
                Renga.IModelObject rengaObject = allObjects.GetByIndex(i);
                if (rengaObject.ObjectType != Renga.EntityTypes.AssemblyInstance) continue;

                Renga.IModel? rengaAssemblyObject_asModel = null;
                // Тут кстати тоже иногда вываивается без try..catch, хотя сборка должна приводиться же к этому интерфейсу
                try
                {
                    rengaAssemblyObject_asModel = rengaObject.GetInterfaceByName("IModel") as Renga.IModel;
                }
                catch { }

                if (rengaAssemblyObject_asModel == null) continue;

                Renga.Cube rengaAssemblyObjectBboxInfo = rengaModel.GetBoundingBox();
                object[] cubeInfo = new object[]
                {
                   rengaAssemblyObjectBboxInfo.GetMinPointMetersStr(),
                   rengaAssemblyObjectBboxInfo.GetMaxPointMetersStr(),
                   rengaAssemblyObjectBboxInfo.GetArea().ToString("0.000"),
                   rengaAssemblyObjectBboxInfo.GetVolume().ToString("0.000")
                };
                // Сохраняем в свойства сборок вычисленные значения ...
            }

            rengaApplication.UI.ShowMessageBox(Renga.MessageIcon.MessageIcon_Info, "Габариты модели", modelBboxInfoText);


        }
    }
}

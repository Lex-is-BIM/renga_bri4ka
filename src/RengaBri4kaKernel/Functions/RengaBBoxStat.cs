using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Renga;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.RengaInternalResources;

using RengaBri4kaKernel.UI.Windows;

namespace RengaBri4kaKernel.Functions
{
    public class RengaBBoxStatStat : PluginParametersCollection
    {
        public static Guid PointMinId = new Guid("{199471e2-4b64-485a-8b7d-6d2608b72b1b}");
        public const string PointMin = "Bri4ka. Левая нижняя точка, метры";

        public static Guid PointMaxId = new Guid("{b3143325-bc42-4dd8-b8c5-cdbc4a802d49}");
        public const string PointMax = "Bri4ka. Правая верхняя точка, метры";

        public static Guid BboxAreaId = new Guid("{750d3bf7-c83d-4637-823a-8f2dbc310793}");
        public const string BboxArea = "Bri4ka. Площадь BBox, м²";

        public static Guid BboxVolumeId = new Guid("{948d9e58-c298-459e-804f-295b76027ca7}");
        public const string BboxVolume = "Bri4ka. Объём BBox, м³";

    }
    public class RengaBBoxStat
    {
        public RengaBBoxStat()
        {
            //зарегистрировать параметры, если ониотсутствуют
            RengaPropertiesUtils.RegisterPropertyIfNotReg(RengaBBoxStatStat.PointMinId, RengaBBoxStatStat.PointMin, PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(RengaBBoxStatStat.PointMaxId, RengaBBoxStatStat.PointMax, PropertyType.PropertyType_String);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(RengaBBoxStatStat.BboxAreaId, RengaBBoxStatStat.BboxArea, PropertyType.PropertyType_Area);
            RengaPropertiesUtils.RegisterPropertyIfNotReg(RengaBBoxStatStat.BboxVolumeId, RengaBBoxStatStat.BboxVolume, PropertyType.PropertyType_Volume);

            RengaPropertiesUtils.AssignPropertiesToTypes(RengaBBoxStatStat.PointMinId, new Guid[] {RengaObjectTypes.AssemblyInstance, RengaEntityTypes.Building});
            RengaPropertiesUtils.AssignPropertiesToTypes(RengaBBoxStatStat.PointMaxId, new Guid[] { RengaObjectTypes.AssemblyInstance, RengaEntityTypes.Building });
            RengaPropertiesUtils.AssignPropertiesToTypes(RengaBBoxStatStat.BboxAreaId, new Guid[] { RengaObjectTypes.AssemblyInstance, RengaEntityTypes.Building });
            RengaPropertiesUtils.AssignPropertiesToTypes(RengaBBoxStatStat.BboxVolumeId, new Guid[] { RengaObjectTypes.AssemblyInstance, RengaEntityTypes.Building });
        }

        public void Calculate()
        {
            Renga.IProject? rengaProject = PluginData.Project;
            if (rengaProject == null) return;

            Renga.IModel rengaModel = rengaProject.Model;
            if (rengaModel == null) return;

            Guid[] propsIds = new Guid[] { RengaBBoxStatStat.PointMinId, RengaBBoxStatStat.PointMaxId, RengaBBoxStatStat.BboxAreaId, RengaBBoxStatStat.BboxVolumeId };

            // Работа с моделью
            Renga.Cube modelBboxInfo = rengaModel.GetBoundingBox();

            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            string modelBboxInfoText = "" +
                $"Минимальная точка, метры = {modelBboxInfo.GetMinPointMetersStr()}\n" +
                $"Максимальная точка, метры = {modelBboxInfo.GetMaxPointMetersStr()}\n" +
                $"Площадь = {modelBboxInfo.GetArea().ToString("0.000")} м²\n" +
                $"Объём = {modelBboxInfo.GetVolume().ToString("0.000")} м³";

            rengaProject.BuildingInfo.GetProperties().SetProperties(propsIds, new object[]
            {
                modelBboxInfo.GetMinPointMetersStr(),
                modelBboxInfo.GetMaxPointMetersStr(),
                modelBboxInfo.GetArea(),
                modelBboxInfo.GetVolume()
            });

            // Работа со сборками
            Renga.IModelObject[]? rengaAssemblies = rengaModel.GetObjects2(new Guid[] { RengaObjectTypes.AssemblyInstance });
            if (rengaAssemblies != null && rengaAssemblies.Any())
            {
                foreach (Renga.IModelObject rengaAssemblyObject in rengaAssemblies)
                {
                    Renga.IModel? rengaAssemblyObject_asModel = null;
                    try
                    {
                        rengaAssemblyObject_asModel = rengaAssemblyObject.GetInterfaceByName("IModel") as Renga.IModel;
                    }
                    catch  { }
                    
                    if (rengaAssemblyObject_asModel != null)
                    {
                        Renga.Cube rengaAssemblyObjectBboxInfo = rengaModel.GetBoundingBox();
                        rengaAssemblyObject.GetProperties().SetProperties(propsIds, new object[]
                        {
                            rengaAssemblyObjectBboxInfo.GetMinPointMetersStr(),
                            rengaAssemblyObjectBboxInfo.GetMaxPointMetersStr(),
                            rengaAssemblyObjectBboxInfo.GetArea().ToString("0.000"),
                            rengaAssemblyObjectBboxInfo.GetVolume().ToString("0.000")
                        });
                    }
                }
            }

            

            editOperation.Apply();

            //Bri4ka_TextForm.ShowTextWindow(modelBboxInfoText, "Габариты модели Renga");
            RengaUtils.ShowMessageBox("Габариты модели Renga\n" + modelBboxInfoText);
        }
    }
}

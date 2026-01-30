using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.RengaInternalResources;

namespace RengaBri4kaKernel.Functions
{
    public class ParametersElevationCalc : PluginParametersCollection
    {
        public static Guid ElevationId = new Guid("{cef2c77a-0c0c-447e-8928-c3971217e88f}");
        public const string Elevation = "Bri4ka. Абсолютная отметка";


    }
    public class RengaElevationCalc
    {
        public RengaElevationCalc()
        {
            //зарегистрировать параметры, если ониотсутствуют
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersElevationCalc.ElevationId, ParametersElevationCalc.Elevation, Renga.PropertyType.PropertyType_Double);

            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersElevationCalc.ElevationId, RengaObjectTypes.GetObject3dCategories());
        }

        public void Calculate()
        {
            Renga.IProject? rengaProject = PluginData.Project;
            if (rengaProject == null) return;

            Renga.IModel? rengaModel = rengaProject.Model;
            if (rengaModel == null) return;

            var rengaAllObjects_3d = rengaModel.GetObjects2(RengaObjectTypes.GetObject3dCategories());
            if (rengaAllObjects_3d == null) return;

            Guid[] propsIds = new Guid[] { ParametersElevationCalc.ElevationId };

            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();


            foreach (Renga.IModelObject rengaObject  in rengaAllObjects_3d)
            {
                bool elevStatus = false;
                double elev = rengaObject.GetElevation(out elevStatus);

                if (elevStatus)
                {
                    rengaObject.SetObjectsProperties(propsIds, new object[] { elev });
                }
            }

            editOperation.Apply();
        }
    }
}

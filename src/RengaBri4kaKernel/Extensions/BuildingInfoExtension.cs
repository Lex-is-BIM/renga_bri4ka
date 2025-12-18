using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{

    internal static class BuildingInfoExtension
    {
        /// <summary>
        /// Влзвращает параметры трансформации координат из свойств здания. Сдвиг XYZ в метрах, угол поворота вокруг оси Z в градусах
        /// </summary>
        /// <param name="buildingInfo"></param>
        /// <returns></returns>
        public static double[] GetTransformPararameters(this Renga.IBuildingInfo buildingInfo)
        {
            double[] trparams = new double[] { 0, 0, 0, 0 };
            if (PluginData.Project == null) return trparams;

            // Резервируем имена целевых свойств
            string propName_IfcLocationX = "IfcLocationX";
            string propName_IfcLocationY = "IfcLocationY";
            string propName_IfcLocationZ = "IfcLocationZ";
            string propName_IfcDirectionPrecession = "IfcDirectionPrecession";

            var propDefs = PluginData.Project.PropertyManager.GetPropertiesDef();
            if (propDefs == null) return trparams;

            // Находим определения свойств в проекте Renga

            var propRes_IfcLocationX = propDefs.Where(p => p.Name == propName_IfcLocationX);
            var propRes_IfcLocationY = propDefs.Where(p => p.Name == propName_IfcLocationY);
            var propRes_IfcLocationZ = propDefs.Where(p => p.Name == propName_IfcLocationZ);
            var propRes_IfcDirectionPrecession = propDefs.Where(p => p.Name == propName_IfcDirectionPrecession);

            // Если хотя бы одного определения нет, завершаем
            if (!propRes_IfcLocationX.Any() || !propRes_IfcLocationY.Any() || !propRes_IfcLocationZ.Any() || !propRes_IfcDirectionPrecession.Any()) return trparams;

            // Идентификаторы свойств
            RengaPropertyDefinition propDef_IfcLocationX = propRes_IfcLocationX.First();
            RengaPropertyDefinition propDef_IfcLocationY = propRes_IfcLocationY.First();
            RengaPropertyDefinition propDef_IfcLocationZ = propRes_IfcLocationZ.First();
            RengaPropertyDefinition propDef_IfcDirectionPrecession = propRes_IfcDirectionPrecession.First();

            Renga.IProperty? propInstance_IfcLocationX = null;
            Renga.IProperty? propInstance_IfcLocationY = null;
            Renga.IProperty? propInstance_IfcLocationZ = null;
            Renga.IProperty? propInstance_IfcDirectionPrecession = null;

            var props = buildingInfo.GetProperties();
            if (props.Contains(propDef_IfcLocationX.Id)) propInstance_IfcLocationX = props.Get(propDef_IfcLocationX.Id);
            if (props.Contains(propDef_IfcLocationY.Id)) propInstance_IfcLocationY = props.Get(propDef_IfcLocationY.Id);
            if (props.Contains(propDef_IfcLocationZ.Id)) propInstance_IfcLocationZ = props.Get(propDef_IfcLocationZ.Id);
            if (props.Contains(propDef_IfcDirectionPrecession.Id)) propInstance_IfcDirectionPrecession = props.Get(propDef_IfcDirectionPrecession.Id);

            if (propInstance_IfcLocationX == null || propInstance_IfcLocationY == null || propInstance_IfcLocationZ == null || propInstance_IfcDirectionPrecession == null) return trparams;

            // поверяем типы свойств
            if (propDef_IfcLocationX.PType != Renga.PropertyType.PropertyType_Length |
                propDef_IfcLocationY.PType != Renga.PropertyType.PropertyType_Length |
                propDef_IfcLocationZ.PType != Renga.PropertyType.PropertyType_Length |
                propDef_IfcDirectionPrecession.PType != Renga.PropertyType.PropertyType_Angle) return trparams;

            trparams[0] = propInstance_IfcLocationX.GetLengthValue(Renga.LengthUnit.LengthUnit_Meters);
            trparams[1] = propInstance_IfcLocationY.GetLengthValue(Renga.LengthUnit.LengthUnit_Meters);
            trparams[2] = propInstance_IfcLocationZ.GetLengthValue(Renga.LengthUnit.LengthUnit_Meters);

            var angle = propInstance_IfcDirectionPrecession.GetAngleValue(Renga.AngleUnit.AngleUnit_Degrees);
            if (angle < 0) angle = 360.0 + angle;
            trparams[3] = Math.PI * propInstance_IfcDirectionPrecession.GetAngleValue(Renga.AngleUnit.AngleUnit_Degrees) / 180.0;

            return trparams;

        }
    }
}

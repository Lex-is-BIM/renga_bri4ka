using Microsoft.Win32;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Geometry;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Functions
{
    public enum RengaCmdCommandVariant
    {
        SET_LEVEL_ACTIVE,
        CREATE_BEAM,
        CREATE_COLUMN,
        CREATE_FLOOR,
        CREATE_ROOF,
        CREATE_WALL,
        CREATE_LINE,
        CREATE_TEXT,
        CREATE_HATCH,
        CREATE_PROPERTY,
        CREATE_PROPERTY2,
        CREATE_PROPERTYENUM,
        CREATE_PROPERTYENUM2
    }

    public class RengaCmdPreProcessor
    {
        public const string CMDSCENARIOS_DIRCTORY = "CmdScenarios";
        public RengaCmdPreProcessor()
        {

        }

        public static string GetCmdScenariosDir()
        {
            string scenariosDir = Path.Combine(PluginConfig.GetDirectoryPath(), CMDSCENARIOS_DIRCTORY);
            if (!Directory.Exists(scenariosDir)) Directory.CreateDirectory(scenariosDir);
            return scenariosDir;
        }

        /// <summary>
        /// Возвращает имена командных сценариев из специльной папки
        /// </summary>
        /// <returns></returns>
        public List<string> GetRegisteredCommands()
        {
            List<string> result = new List<string>();

            foreach (string scenarioFile in Directory.GetFiles(GetCmdScenariosDir(), "*.txt", SearchOption.TopDirectoryOnly))
            {
                result.Add(Path.GetFileNameWithoutExtension(scenarioFile));
            }

            return result;
        }

        public void RunRegisteredScenarion(string scenarioName)
        {
            string scenarioFile = Path.Combine(GetCmdScenariosDir(), scenarioName + ".txt");
            if (!File.Exists(scenarioFile)) return;

            RunCommand(File.ReadAllText(scenarioFile));
        }

        public void SaveScenario(string text)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Сохранение сценария";
            saveFileDialog.Filter = "Сценарий (*.txt) | *.txt";
            saveFileDialog.AddExtension = true;

            saveFileDialog.InitialDirectory = Path.Combine(PluginConfig.GetDirectoryPath(), GetCmdScenariosDir());
            if (!Directory.Exists(saveFileDialog.InitialDirectory)) Directory.CreateDirectory(saveFileDialog.InitialDirectory);

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, text + "\n");
            }
        }

        public void RunCommand(string? commandRaw)
        {
            if (commandRaw == null) return;
            string command = commandRaw.Replace("\r", "");
            if (command.Contains("\n"))
            {
                string[] commands = command.Split('\n');
                foreach (string commandLine in commands)
                {
                    RunCommand(commandLine);
                }
                return;
            }

            if (command.StartsWith("::")) return;
            if (!command.Contains("(")) return;
            string commandPrefix = command.Substring(0, command.IndexOf("(")).Replace(" ", "");

            RengaCmdCommandVariant? commandVar = (RengaCmdCommandVariant?)Enum.Parse(typeof(RengaCmdCommandVariant), commandPrefix);
            if (commandVar == null)
            {
                RengaUtils.ShowMessageBox("Команда не определена!", true);
                return;
            }

            Renga.IModel? rengaModel = PluginData.GetModel();
            if (rengaModel == null) return;

            string commandArguments = command.Substring(command.IndexOf("(") + 1, command.LastIndexOf(")") - command.IndexOf("(") - 1);
            switch (commandVar)
            {
                case RengaCmdCommandVariant.SET_LEVEL_ACTIVE:
                    {
                        this.mActiveLayerId = rengaModel.GetLevelIdByName(commandArguments);
                        break;
                    }
                case RengaCmdCommandVariant.CREATE_BEAM:
                    {
                        if (!commandArguments.Contains(";")) return;
                        Point3D? pStart = GetSinglePoint(commandArguments.Split(';')[0]);
                        Point3D? pEnd = GetSinglePoint(commandArguments.Split(';')[1]);
                        if (pStart == null || pEnd == null) return;

                        Renga.IModelObject? modelObject = rengaModel.CreateBaselineObject(BaselineObjectType.Beam, new List<Point3D>() { pStart, pEnd }, mActiveLayerId, false);
                        break;
                    }
                case RengaCmdCommandVariant.CREATE_COLUMN:
                    {
                        if (!commandArguments.Contains(";")) return;
                        Point3D? position = GetSinglePoint(commandArguments.Split(';')[0]);
                        double height = 1000;
                        double.TryParse(commandArguments.Split(';')[1], out height);
                        if (position == null) return;


                        Renga.IModelObject? modelObject = rengaModel.CreatePositionObject(SinglePositionObjectType.Column, position, mActiveLayerId, false);
                        if (modelObject == null) return;
                        if (modelObject.ObjectType != RengaObjectTypes.Column) return;

                        var op = PluginData.Project.CreateOperation();
                        op.Start();
                        var paramCollection = modelObject.GetParameters();
                        if (paramCollection == null)
                        {
                            op.Rollback();
                            break;
                        }
                        if (paramCollection.Contains(Renga.Parameters.ColumnHeight))
                        {
                            var param = paramCollection.Get(Renga.Parameters.ColumnHeight);
                            if (param == null)
                            {
                                op.Rollback();
                                break;
                            }
                            param.SetDoubleValue(height);
                        }
                        

                        op.Apply();
                        break;
                    }
                case RengaCmdCommandVariant.CREATE_FLOOR:
                case RengaCmdCommandVariant.CREATE_WALL:
                case RengaCmdCommandVariant.CREATE_ROOF:
                case RengaCmdCommandVariant.CREATE_HATCH:
                case RengaCmdCommandVariant.CREATE_LINE:
                    {
                        double thickness = 1000.0;
                        if (commandVar != RengaCmdCommandVariant.CREATE_HATCH && commandVar != RengaCmdCommandVariant.CREATE_LINE)
                        {
                            if (!commandArguments.Contains(";")) return;
                            double.TryParse(commandArguments.Split(';')[1], out thickness);
                        }

                        double height = 3000.0;
                        if (commandVar == RengaCmdCommandVariant.CREATE_WALL)
                        {
                            if (!commandArguments.Contains(";")) return;
                            double.TryParse(commandArguments.Split(';')[2], out height);
                        }

                        List<Point3D>? contour = GetPoints(commandArguments.Split(';')[0]);
                        if (contour == null || contour.Count < 2) return;

                        BaselineObjectType oType = BaselineObjectType.Wall;
                        if (commandVar == RengaCmdCommandVariant.CREATE_FLOOR) oType = BaselineObjectType.Floor;
                        else if (commandVar == RengaCmdCommandVariant.CREATE_WALL) oType = BaselineObjectType.Wall;
                        else if (commandVar == RengaCmdCommandVariant.CREATE_ROOF) oType = BaselineObjectType.Roof;
                        else if (commandVar == RengaCmdCommandVariant.CREATE_HATCH) oType = BaselineObjectType.Hatch;
                        else if (commandVar == RengaCmdCommandVariant.CREATE_LINE)
                        {
                            oType = BaselineObjectType.Line3d;
                            if (PluginData.rengaApplication.ActiveView.Type == Renga.ViewType.ViewType_Drawing) oType = BaselineObjectType.DrawingLine;
                        }

                        Renga.IModelObject? modelObject = rengaModel.CreateBaselineObject(oType, contour, mActiveLayerId, false);
                        if (modelObject == null) return;

                        var op = PluginData.Project.CreateOperation();
                        op.Start();
                        var paramsCollection = modelObject.GetParameters();
                        Renga.IParameter? needParam = null;
                        if (commandVar == RengaCmdCommandVariant.CREATE_FLOOR && paramsCollection.Contains(Renga.Parameters.FloorThickness)) needParam = paramsCollection.Get(Renga.Parameters.FloorThickness);
                        else if (commandVar == RengaCmdCommandVariant.CREATE_WALL && paramsCollection.Contains(Renga.Parameters.WallThickness)) needParam = paramsCollection.Get(Renga.Parameters.WallThickness);
                        else if (commandVar == RengaCmdCommandVariant.CREATE_ROOF && paramsCollection.Contains(Renga.Parameters.RoofThickness)) needParam = paramsCollection.Get(Renga.Parameters.RoofThickness);
                        if (needParam != null) needParam.SetDoubleValue(thickness);
                        
                        op.Apply();
                        break;
                    }
                case RengaCmdCommandVariant.CREATE_TEXT:
                    {
                        if (!commandArguments.Contains(";")) return;
                        Point3D? position = GetSinglePoint(commandArguments.Split(';')[0]);
                        if (position == null) return;

                        TextObjectType textType = TextObjectType.ModelText;
                        if (PluginData.rengaApplication.ActiveView.Type == Renga.ViewType.ViewType_Drawing) textType = TextObjectType.DrawingText;

                        Renga.IModelObject? modelObject = rengaModel.CreateText(position, commandArguments.Split(';')[1], textType, this.mActiveLayerId);


                        break;
                    }
                case RengaCmdCommandVariant.CREATE_PROPERTY:
                case RengaCmdCommandVariant.CREATE_PROPERTY2:
                case RengaCmdCommandVariant.CREATE_PROPERTYENUM:
                case RengaCmdCommandVariant.CREATE_PROPERTYENUM2:
                    {
                        if (!commandArguments.Contains(";")) return;
                        string[] propDef = commandArguments.Split(';');

                        if ((commandVar == RengaCmdCommandVariant.CREATE_PROPERTY | commandVar == RengaCmdCommandVariant.CREATE_PROPERTYENUM) && propDef.Length != 3) return;
                        if ((commandVar == RengaCmdCommandVariant.CREATE_PROPERTY2 | commandVar == RengaCmdCommandVariant.CREATE_PROPERTYENUM2) && propDef.Length != 4) return;

                        // Имя свойства
                        string propName = propDef[0];

                        // Тип свойства
                        Renga.PropertyType propType = Renga.PropertyType.PropertyType_Undefined;

                        if (commandVar == RengaCmdCommandVariant.CREATE_PROPERTY | commandVar == RengaCmdCommandVariant.CREATE_PROPERTY2)
                        {
                            string propTypeStr = "";
                            if (commandVar == RengaCmdCommandVariant.CREATE_PROPERTY) propTypeStr = propDef[1];
                            else propTypeStr = propDef[2];

                            int propTypeRaw = int.Parse(propTypeStr);
                            if (Enum.TryParse<Renga.PropertyType>(propTypeStr, out Renga.PropertyType propTypeResult))
                            {
                                propType = propTypeResult;
                            }
                        }
                        else propType = Renga.PropertyType.PropertyType_Enumeration;
                        if (propType == Renga.PropertyType.PropertyType_Undefined) return;

                        // Идентификатор свойства
                        Guid propId = Guid.NewGuid();

                        if (commandVar == RengaCmdCommandVariant.CREATE_PROPERTY2 | commandVar == RengaCmdCommandVariant.CREATE_PROPERTYENUM2)
                        {
                            string guidRaw = propDef[1];
                            if (Guid.TryParse(guidRaw, out Guid guidResult)) propId = guidResult;
                        }

                        // Перечисление

                        string[]? enumsVariants = null;
                        if (commandVar == RengaCmdCommandVariant.CREATE_PROPERTYENUM | commandVar == RengaCmdCommandVariant.CREATE_PROPERTYENUM2)
                        {
                            string enums = propDef[1];
                            if (commandVar == RengaCmdCommandVariant.CREATE_PROPERTYENUM2) enums = propDef[2];
                            if (enums == "") return;

                            if (!enums.Contains(" ")) enumsVariants = new string[] { enums };
                            else enumsVariants = enums.Split(' ');
                        }

                        RengaPropertiesUtils.RegisterPropertyIfNotReg(propId, propName, propType, enumsVariants);

                        // Категории объектов Renga, кому назначены свойства
                        Guid[]? objectTypes = null;

                        string propAssignTypes = "";
                        if (commandVar == RengaCmdCommandVariant.CREATE_PROPERTY | commandVar == RengaCmdCommandVariant.CREATE_PROPERTYENUM) propAssignTypes = propDef[2];
                        else if (commandVar == RengaCmdCommandVariant.CREATE_PROPERTY2 | commandVar == RengaCmdCommandVariant.CREATE_PROPERTYENUM2) propAssignTypes = propDef[3];

                        if (propAssignTypes != "")
                        {
                            string[] propAssignTypes2;
                            if (propAssignTypes.Contains(" ")) propAssignTypes2 = propAssignTypes.Split(' ');
                            else propAssignTypes2 = new string[] { propAssignTypes };

                            List<Guid> objectTypesTemp = new List<Guid>();

                            var types = typeof(Renga.EntityTypes).GetRuntimeFields();
                            foreach (string type in propAssignTypes2)
                            {
                                foreach (var rengaEntityType in types)
                                {
                                    string rengaEntityType2 = rengaEntityType.Name.Replace("<", "");
                                    rengaEntityType2 = rengaEntityType2.Substring(0, rengaEntityType2.IndexOf(">"));

                                    if (type == rengaEntityType2)
                                    {
                                        Guid? guid = (Guid?)rengaEntityType.GetValue(null);
                                        if (guid != null) objectTypesTemp.Add(guid.Value);
                                    }
                                }
                            }
                            objectTypes = objectTypesTemp.ToArray();
                        }

                        RengaPropertiesUtils.AssignPropertiesToTypes(propId, objectTypes);
                        break;
                    }
            }
        }

        private Point3D? GetSinglePoint(string argsRaw)
        {
            if (!argsRaw.Contains(" ")) return null;
            string args = argsRaw.TrimEnd().TrimStart();

            string[] numberComponents = args.Split(' ');
            if (numberComponents.Length > 1)
            {
                double x = 0.0, y = 0.0, z = 0.0;
                double.TryParse(numberComponents[0], out x);
                double.TryParse(numberComponents[1], out y);
                if (numberComponents.Length == 3) double.TryParse(numberComponents[2], out z);

                return new Point3D(x, y, z);
            }
            return null;
        }

        private List<Point3D>? GetPoints(string args)
        {
            if (!args.Contains(",")) return null;
            string[] numbers = args.Split(',');
            List<Point3D> points = new List<Point3D>();

            foreach (string number in numbers)
            {
                Point3D? p = GetSinglePoint(number);
                if (p == null) continue;
                points.Add(p);
            }
            return points;
        }


        private int mActiveLayerId = -1;
    }
}

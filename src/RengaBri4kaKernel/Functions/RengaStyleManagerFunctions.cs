using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.Functions;

namespace RengaBri4kaKernel.Configs
{
    public partial class RengaStyleManagerConfig : RengaStyleManagerConfigFunc
    {
        private const string RST_Entry_Version = "version";
        private const string RST_Entry_Parameters = "parameters.json";
        public void AddRengaProject(string filePath, bool reSave = false)
        {
            Guid fileId = Guid.Empty;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName.StartsWith(RSM_ProjectFilePrefix))
            {
                string fileNameEd = fileName.Substring(fileName.IndexOf(RSM_ProjectFilePrefix));
                if (Guid.TryParse(fileNameEd, out fileId))
                {
                    // Среди имеющихся проектов не должно быть файла с тем же кодом
                    var extProjects = this.ProjectsCollection.Where(project => project.Id.Equals(fileId));
                    if (extProjects.Any()) return;
                }
            }
            fileId = Guid.NewGuid();
            fileName = RSM_ProjectFilePrefix + fileId.ToString("N");

            string fileNameExt = fileName + Path.GetExtension(filePath).ToLower();
            string projectPath = Path.Combine(GetProjectsDirectory(), fileNameExt);
            File.Copy(filePath, projectPath, true);

            //---Начинаем работать с файлом
            RengaProjectWithObjects projectFileDef = new RengaProjectWithObjects();
            projectFileDef.FileNameOrigin = Path.GetFileNameWithoutExtension(filePath);
            projectFileDef.Id = fileId;
            

            // read File metadata
            RengaFileExplorer projectExpl = new RengaFileExplorer(projectPath);
            projectFileDef.ProjectVersion = new Version(projectExpl.GetLongVersion());

            // Iterate
            if (PluginData.rengaApplication.OpenProject(projectPath) != 0) return;
            if (PluginData.Project == null) return;

            // Считываем стили.
            tmpStylesCollection = new List<RengaObjectStyleInProject>();
            GetFromEntityCollection(PluginData.Project.WindowStyles, fileId, RengaStyleCategoryVariant.Window);
            GetFromEntityCollection(PluginData.Project.DoorStyles, fileId, RengaStyleCategoryVariant.Door);
            GetFromEntityCollection(PluginData.Project.ElementStyles, fileId, RengaStyleCategoryVariant.Element);

            GetFromEntityCollection(PluginData.Project.ColumnStyles, fileId, RengaStyleCategoryVariant.Column);
            GetFromEntityCollection(PluginData.Project.BeamStyles, fileId, RengaStyleCategoryVariant.Beam);
            GetFromEntityCollection(PluginData.Project.PlateStyles, fileId, RengaStyleCategoryVariant.Plate);
            GetFromEntityCollection(PluginData.Project.HoleStyles, fileId, RengaStyleCategoryVariant.Hole);

            GetFromEntityCollection(PluginData.Project.RebarStyles, fileId, RengaStyleCategoryVariant.Rebar);
            GetFromEntityCollection(PluginData.Project.ReinforcementUnitStyles, fileId, RengaStyleCategoryVariant.ReinforcementUnit);
            GetFromEntityCollection(PluginData.Project.ReinforcementStyles, fileId, RengaStyleCategoryVariant.Reinforcement);
            //GetFromEntityCollection(PluginData.Project.HoleStyles, fileId, RengaStyleCategoryVariant.Reinforcment_Connection);

            GetFromEntityCollection(PluginData.Project.PlumbingFixtureStyles, fileId, RengaStyleCategoryVariant.PlumbingFixture);
            GetFromEntityCollection(PluginData.Project.EquipmentStyles, fileId, RengaStyleCategoryVariant.Equipment);
            GetFromEntityCollection(PluginData.Project.PipeStyles, fileId, RengaStyleCategoryVariant.Pipe);
            GetFromEntityCollection(PluginData.Project.PipeFittingStyles, fileId, RengaStyleCategoryVariant.PipeFitting);
            GetFromEntityCollection(PluginData.Project.PipeAccessoryStyles, fileId, RengaStyleCategoryVariant.PipeAccessory);

            GetFromEntityCollection(PluginData.Project.DuctStyles, fileId, RengaStyleCategoryVariant.Duct);
            GetFromEntityCollection(PluginData.Project.DuctFittingStyles, fileId, RengaStyleCategoryVariant.DuctFitting);
            GetFromEntityCollection(PluginData.Project.DuctAccessoryStyles, fileId, RengaStyleCategoryVariant.DuctAccessory);

            GetFromEntityCollection(PluginData.Project.WiringAccessoryStyles, fileId, RengaStyleCategoryVariant.WiringAccessory);
            GetFromEntityCollection(PluginData.Project.LightingFixtureStyles, fileId, RengaStyleCategoryVariant.LightingFixture);
            GetFromEntityCollection(PluginData.Project.ElectricDistributionBoardStyles, fileId, RengaStyleCategoryVariant.ElectricDistributionBoard);
            GetFromEntityCollection(PluginData.Project.ElectricalConductorStyles, fileId, RengaStyleCategoryVariant.ElectricalConductor);
            GetFromEntityCollection(PluginData.Project.ElectricalCircuitLineStyles, fileId, RengaStyleCategoryVariant.ElectricalCircuitLine);

            // Считывем объекты и сравниваем со стилями. Как правило, для каждого стиля присутствует вхождение, но это не обязательное условие

            Renga.IModelObjectCollection rengaObjects = PluginData.Project.Model.GetObjects();
            for (int objIndex = 0; objIndex < rengaObjects.Count; objIndex++)
            {
                Renga.IModelObject rengaObject = rengaObjects.GetByIndex(objIndex);
                var paramDescr = mRengaStylesInfo.Where(s => s.RengaObjectType == rengaObject.ObjectType);
                if (!paramDescr.Any()) continue;

                Renga.IParameter? rengaParam = null;
                try
                {
                    rengaParam = rengaObject.GetParameters().Get(paramDescr.First().RengaStyleParameterId);
                }
                catch (Exception ex) { }

                if (rengaParam == null) continue;
                string styleName = rengaParam.GetStringValue();

                // Получаем тип стиля
                RengaStyleCategoryVariant styleCategoryType = paramDescr.First().Bri4kaStyleType;

                //Нужно перебрать tmpStylesCollection
                int index = tmpStylesCollection.FindIndex(x => x.Name == styleName && x.StyleCategory == styleCategoryType);
                if (index != -1)
                {
                    tmpStylesCollection[index].ObjectUnuqueId = rengaObject.UniqueId;
                    //tmpStylesCollection[index].IconPath = "";
                    // Hide all other objects, center camera on object
                }
            }

            //Сохраняем tmpStylesCollection в коллекцию стилей
            StylesCollecion = StylesCollecion.Concat(tmpStylesCollection).ToList();


            // 1. Save changes in current project
            PluginData.rengaApplication.CloseProject2();

            if (reSave) 
            {
                PluginData.rengaApplication.Project.Save();
                projectFileDef.ProjectVersion = PluginData.RengaVersion;
            }

            this.ProjectsCollection.Add(projectFileDef);
           
            PluginData.rengaApplication.CloseProject(!reSave);
        }

        private List<RengaObjectStyleInProject> tmpStylesCollection;

        /// <summary>
        /// Вспомогательный метод. Считывает набор стилей и формирует их описания
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        private void GetFromEntityCollection(Renga.IEntityCollection entities, Guid projectId, RengaStyleCategoryVariant category)
        {
            //List<RengaObjectStyleInProject> styles = new List<RengaObjectStyleInProject>();
            for (int styleCounter = 0; styleCounter < entities.Count; styleCounter++)
            {
                Renga.IEntity styleEntity = entities.GetByIndex(styleCounter);
                RengaObjectStyleInProject styleDef = new RengaObjectStyleInProject();
                styleDef.ProjectId = projectId;
                styleDef.StyleCategory = category;
                styleDef.Name = styleEntity.Name;
                styleDef.Properties = new Dictionary<string, string>();

                var entityProperties = styleEntity as Renga.IPropertyContainer;
                if (entityProperties != null) styleDef.Properties = entityProperties.GetPropertiesStr();

                tmpStylesCollection.Add(styleDef);
            }
        }

        public bool IsStyleExists(RengaStyleDef style)
        {
            RengaSTDLFile? style_STDL = style as RengaSTDLFile;
            RengaObjectStyleInProject? style_Project = style as RengaObjectStyleInProject;

            foreach (var styleDef in this.StylesCollecion)
            {
                if (styleDef.GetStyleType() == StyleTypeVariant.STDL)
                {
                    RengaSTDLFile? styleDef_STDL = styleDef as RengaSTDLFile;
                    if (styleDef_STDL == null) continue;

                    if (styleDef_STDL.Equals(style_STDL)) return true;


                }
                else if (styleDef.GetStyleType() == StyleTypeVariant.ObjectInProject)
                {
                    RengaObjectStyleInProject? styleDef_Project = styleDef as RengaObjectStyleInProject;
                    if (styleDef_Project == null) continue;

                    if (styleDef_Project.Equals(style_Project)) return true;
                }
            }

            return false;
        }
        //public void AddObjectInProject(RengaObjectStyleInProject objectDef);
        public void AddStdlObject(string stdlPath)
        {
            if (Path.GetExtension(stdlPath).ToLower() != ".rst") return;
            //просто так проверить, есть ли такое определение нельзя -- придется сперва копировать и считывать архив

            string rstPath = Path.Combine(GetStdlDirectory(), Guid.NewGuid().ToString("N") + ".rst");
            File.Copy(stdlPath, rstPath, true);

            RengaSTDLFile rstDef = new RengaSTDLFile();
            
            bool rstReadStatus = false;
            using (var rnpAsZip = ZipFile.OpenRead(rstPath))
            {
                foreach (var entry in rnpAsZip.Entries)
                {
                    if (entry.Name == RST_Entry_Version)
                    {
                        var osr = new StreamReader(entry.Open(), Encoding.Default);
                        rstDef.STDL_Version = new Version(osr.ReadToEnd());
                        osr.Close();
                    }
                    else if (entry.Name == RST_Entry_Parameters)
                    {
                        var osr = new StreamReader(entry.Open(), Encoding.Default);
                        RengaSTDLFileParameters? rstParams = RengaSTDLFileParameters.ReadFrom(osr.ReadToEnd());
                        if (rstParams == null) break;
                        rstReadStatus = true;

                        rstDef.DefaultName = rstParams.metadata.defaultName;
                        rstDef.Description = rstParams.metadata.description;
                        rstDef.Version = rstParams.metadata.version;
                        rstDef.Author = rstParams.metadata.author;

                        foreach (var paramGroup in rstParams.styleParameters)
                        {
                            foreach (var param in paramGroup.parameters)
                            {
                                if (param.type.ToLower() == "string") rstDef.Properties.Add(param.text, param.defaultValue?.ToString() ?? "");
                                else if (param.type.ToLower() == "id") rstDef.Properties.Add(param.text, param.entityTypeId ?? "");
                            }
                        }
                    }
                }
            }

            if (rstReadStatus && !IsStyleExists(rstDef)) this.StylesCollecion.Add(rstDef);
        }

        public void Restore()
        {
            foreach (string rengaProjectPath in Directory.GetFiles(GetProjectsDirectory(), "*.*", SearchOption.TopDirectoryOnly))
            {
                string ext = Path.GetExtension(rengaProjectPath).ToLower();
                if (ext != ".rnp" && ext != ".rnt") continue;
                AddRengaProject(rengaProjectPath);
            }

            foreach (string stdlPath in Directory.GetFiles(GetStdlDirectory(), "*.*", SearchOption.TopDirectoryOnly))
            {
                string ext = Path.GetExtension(stdlPath).ToLower();
                if (ext != ".rst" ) continue;
                AddStdlObject(stdlPath);
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;
using Renga;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.RengaInternalResources;

namespace RengaBri4kaKernel.Functions
{
    public class ParametersSlopeAnalyzing : PLuginParametersCollection
    {
        // Свойства, назначаемые объекту
        public static Guid SlopeMinValueId = new Guid("{21a0cf05-d6d2-4a57-b07f-9163414510a4}");
        public const string SlopeMinValue = "Bri4ka. Минимальный уклон";

        public static Guid SlopeMaxValueId = new Guid("{21a0cf05-d6d2-4a57-b07f-9163414510a4}");
        public const string SlopeMaxValue = "Bri4ka. Максимальный уклон";

        // Свойства, назначаемые текстовому объекту
        public static Guid SlopeText2ObjectId = new Guid("{061a0532-a7bf-40c4-b251-42b11216a401}");
        public const string SlopeText2Object = "Bri4ka. Идентификатор измеряемого элемента модели";
    }
    /// <summary>
    /// Действия по анализу уклонов граней
    /// </summary>
    /// 
    public class RengaGridSlopeAnalyzing
    {
        public RengaGridSlopeAnalyzing()
        {
            mObjectIds = new int[] { };
            mConfig = new GridSlopeAnalyzingConfig();

            RengaUtils.RegisterPropertyIfNotReg(ParametersSlopeAnalyzing.SlopeText2ObjectId, ParametersSlopeAnalyzing.SlopeText2Object, PropertyType.PropertyType_Integer);
            RengaUtils.AssignPropertiesToTypes(ParametersSlopeAnalyzing.SlopeText2ObjectId, new Guid[] {RengaObjectTypes.TextObject});
        }

        public void SetInputData(int[] ids, GridSlopeAnalyzingConfig config)
        {
            mObjectIds = ids;
            mConfig = config;
        }

        private void RegisterPropertiesToObjects()
        {
            RengaUtils.RegisterPropertyIfNotReg(ParametersSlopeAnalyzing.SlopeMinValueId, ParametersSlopeAnalyzing.SlopeMinValue, PropertyType.PropertyType_Double);
            RengaUtils.RegisterPropertyIfNotReg(ParametersSlopeAnalyzing.SlopeMaxValueId, ParametersSlopeAnalyzing.SlopeMaxValue, PropertyType.PropertyType_Double);

            RengaUtils.AssignPropertiesToTypes(ParametersSlopeAnalyzing.SlopeMinValueId, null);
            RengaUtils.AssignPropertiesToTypes(ParametersSlopeAnalyzing.SlopeMaxValueId, null);
        }

        public void Calculate()
        {
            if (mObjectIds == null) return;
            //
            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            TimerUtils.CreateInstance().Start();
            Renga.IModelObjectCollection modelObjects = PluginData.Project.Model.GetObjects();

            if (mConfig.SaveExtremeResultsToProperties) RegisterPropertiesToObjects();
            Guid[] propIds_Object = new Guid[] { ParametersSlopeAnalyzing.SlopeMinValueId, ParametersSlopeAnalyzing.SlopeMaxValueId };
            Guid[] propIds_Text = new Guid[] { ParametersSlopeAnalyzing.SlopeText2ObjectId };

            double zMax = -1000000.0;
            // Рассчитываем метки
            List<SlopeMarkInfo> slopeMarks = new List<SlopeMarkInfo>();
            foreach (int id in mObjectIds)
            {
                Renga.IModelObject rengaObject = modelObjects.GetById(id);
                Renga.IExportedObject3D? rengaObjectGeometry = rengaObject.GetExportedObject3D();
                if (rengaObjectGeometry == null) continue;

                double minSlope = 10000000.0;
                double maxSlope = -10000000.0;

                for (int rengaMeshCounter = 0; rengaMeshCounter < rengaObjectGeometry.MeshCount; rengaMeshCounter++)
                {
                    Renga.IMesh mesh = rengaObjectGeometry.GetMesh(rengaMeshCounter);

                    for (int rengaGridCounter = 0; rengaGridCounter < mesh.GridCount; rengaGridCounter++)
                    {
                        Renga.IGrid grid = mesh.GetGrid(rengaGridCounter);
                        if (rengaObject.ObjectType == RengaObjectTypes.Roof && grid.GridType != (int)RoofGridType.Top) continue;
                        if (rengaObject.ObjectType == RengaObjectTypes.Ramp && grid.GridType != (int)RampGridTypes.Top) continue;
                        //TODO: другие типы сеток для других объектов? + реализовать их также в Enum

                        Dictionary<int, double[]> vertices = new Dictionary<int, double[]>();

                        for (int rengaVertexCounter = 0; rengaVertexCounter < grid.VertexCount; rengaVertexCounter++)
                        {
                            Renga.FloatPoint3D p = grid.GetVertex(rengaVertexCounter);
                            // Округляем и переводим в м. (по умолчанию в Renga в мм)
                            vertices.Add(rengaVertexCounter, new double[] {
                                Math.Round(p.X, 3) ,
                                Math.Round(p.Y, 3),
                                Math.Round(p.Z, 3) });

                            if (zMax < p.Z) zMax = p.Z;
                        }
                        for (int rengaFaceCounter = 0; rengaFaceCounter < grid.TriangleCount; rengaFaceCounter++)
                        {
                            Renga.Triangle tr = grid.GetTriangle(rengaFaceCounter);
                            TriangleStat trStat = new TriangleStat(vertices[(int)tr.V0], vertices[(int)tr.V1], vertices[(int)tr.V2], mConfig.Units);
                            //Перерводим в м2 для сравнения с полями
                            double trArea = trStat.Area / 1000.0 / 1000.0;
                            
                            trStat.Calculate();
                            if (mConfig.IgnoreTrianglesSquareMore == true && trArea > mConfig.IgnoringTrianglesSquareMore) continue;
                            if (mConfig.IgnoreTrianglesSquareLess == true && trArea < mConfig.IgnoringTrianglesSquareLess) continue;
                            if (mConfig.IgnoreValuesMore == true && trStat.Slope > mConfig.IgnoringValuesMore) continue;
                            if (mConfig.IgnoreValuesLess == true && trStat.Slope < mConfig.IgnoringValuesLess) continue;

                            if (mConfig.SaveExtremeResultsToProperties)
                            {
                                if (minSlope > trStat.Slope) minSlope = trStat.Slope;
                                if (maxSlope < trStat.Slope) maxSlope = trStat.Slope;
                            }

                            slopeMarks.Add(new SlopeMarkInfo()
                            {
                                Position = trStat.Center,
                                Angle = trStat.Angle,
                                ModelObjectId = id,
                                Slope = Math.Round(trStat.Slope, 1)
                            });
                        }
                    }
                }
                if (mConfig.SaveExtremeResultsToProperties) rengaObject.SetObjectsProperties(propIds_Object, new object[] { minSlope, maxSlope });
            }

            editOperation.Apply();

            zMax += 100;

            // Создаем текстовые объекты для меток
            Renga.IModel rengaModel = PluginData.Project.Model;

            int hostObjectId = -1;
            if (mConfig.CreateNewLevelForResults)
            {
                //Для создания уровня нужна транзакция
                editOperation = PluginData.Project.CreateOperationWithUndo(rengaModel.Id);
                editOperation.Start();

                
                var args = rengaModel.CreateNewEntityArgs();
                var pl3d = new Placement3D();
                pl3d.Init(0, 0, zMax);
                args.TypeId = RengaObjectTypes.Level;
                args.Placement3D = pl3d;

                Renga.IModelObject? levelObjectRaw = rengaModel.CreateObject(args);
                
                if (levelObjectRaw != null)
                {
                    hostObjectId = levelObjectRaw.Id;
                    Renga.ILevel? levelObject = levelObjectRaw as Renga.ILevel;
                    if (levelObject != null)
                    {
                        levelObject.Placement.Move(new Vector3D() { X = 0, Y = 0, Z = zMax });
                        //levelObject.LevelName = "";
                        //levelObject.Elevation = maxZ * 1000;
                    }
                }
                editOperation.Apply();
            }

            editOperation = PluginData.Project.CreateOperationWithUndo(rengaModel.Id);
            editOperation.Start();

            foreach (SlopeMarkInfo slopeMark in slopeMarks)
            {
                var args = rengaModel.CreateNewEntityArgs();
                args.TypeId = RengaObjectTypes.ModelText;
                var pl3d = new Placement3D();
                pl3d.Origin = new Point3D { X = slopeMark.Position[0], Y = slopeMark.Position[1], Z = zMax };
                pl3d.xAxis = new Vector3D() { X = 1, Y = 0, Z = 0 };
                pl3d.xAxis = new Vector3D { X = Math.Cos(slopeMark.Angle), Y = Math.Sin(slopeMark.Angle), Z = 0 };
                pl3d.zAxis = new Vector3D { X = 0, Y = 0, Z = 1 };
                args.Placement3D = pl3d;
                //args.HostObjectId = hostObjectId;

                //В рамках новой транзакци создаем метки
                var textObjectRaw = rengaModel.CreateObject(args);

                if (textObjectRaw == null) continue;
                Renga.ITextObject? textObject = textObjectRaw as Renga.ITextObject;
                if (textObject == null) continue;

                Renga.IRichTextDocument textData = textObject.GetRichTextDocument();
                RichTextToken slopeInfo = new RichTextToken()
                {
                    FontFamily = "Arial",
                    FontCapSize = 3,
                    FontColor = new Renga.Color() { Red = 51, Green = 102, Blue = 0, Alpha = 255 },
                    //FontStyle = new FontStyle() { },
                    Text = slopeMark.Slope.ToString() + "→"
                };
                Array tokens = new RichTextToken[] { slopeInfo };
                IRichTextParagraph? testParagraph = textData.AppendParagraph(tokens);

                //Нужно сдвинуть текст на величину огран. рамки
                
                Renga.ILevelObject? textObjectOnLevel = textObjectRaw as Renga.ILevelObject;
                if (textObjectOnLevel != null)
                {
                    var rect = textObject.BoundRect;
                    var pl = textObjectOnLevel.GetPlacement();
                    //Сначала сдвигаем на середину ограничивающей рамки текста
                    pl.Move(new Vector3D()
                    {
                        X = 0,
                        Y = -rect.Right,//- rect.Right,
                        Z = 0
                    });
                    //Затем поворачиваем относительно сдвинутого центра на нужный угол
                    //textObjectOnLevel.SetPlacement(pl);

                    //pl = textObjectOnLevel.GetPlacement();
                    
                    pl.Rotate2(new Point3D()
                    {
                        X = pl.Origin.X,
                        Y = pl.Origin.Y + rect.Right,
                        Z = pl.Origin.Z
                    },
                    new Vector3D() { X = 1, Y = 0, Z = 0 }, slopeMark.Angle / Math.PI * 180.0);
                    
                    //pl.Rotate(new Vector3D() { X = 1, Y = 0, Z = 0 }, slopeMark.Angle);

                    //pl.Move(new Vector3D() { X = - rect.Right/2 * Math.Cos(slopeMark.Angle), Y = - rect.Top/2* Math.Sin(slopeMark.Angle), Z = 0 });
                    //textObjectOnLevel.SetPlacement(pl);
                }
                
            }

            editOperation.Apply();
            //TimerUtils.CreateInstance().Stop();
        }

        internal int[]? mObjectIds;
        internal GridSlopeAnalyzingConfig mConfig;
    }

    class SlopeMarkInfo
    {
        public int ModelObjectId { get; set; }
        public double[] Position { get; set; }
        public double Angle { get; set; }
        public double Slope { get; set; }
    }
}

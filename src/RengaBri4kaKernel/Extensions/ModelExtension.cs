using Renga;
using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Geometry;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RengaBri4kaKernel.Extensions
{
    public enum TextOffsetMode
    {
        NoTransform,
        Center,
        TopLeftCorner
    }
    public enum TextObjectType
    {
        ModelText,
        DrawingText
    }

    public enum BaselineObjectType
    {
        Hatch,
        Line3d,
        DrawingLine,
        Floor,
        Beam,
        Wall,
        Roof
    }

    public enum SinglePositionObjectType
    {
        Equipment,
        Column
    }


    internal static class ModelExtension
    {
        /// <summary>
        /// Создает тектовый объект в пространстве модели или чертежа с заданными настройками и применяет к тексту смещение, если необходимо
        /// </summary>
        /// <param name="rengaModel">Оболочка модели или пространства чертежа</param>
        /// <param name="Position">Точка вставки текста в мм</param>
        /// <param name="text">Значение текста</param>
        /// <param name="textType">Тип текста (модельный или чертежный)</param>
        /// <param name="angleRadians">Угол поворота текста относительно точки начала</param>
        /// <param name="LevelId">ВНутренний идентификатор уровня Renga (для текста модели)</param>
        /// <param name="mode">Тип пост-трансформации координат</param>
        /// <param name="textStyle">Стиль текста</param>
        /// <returns></returns>
        public static Renga.IModelObject? CreateText(
            this Renga.IModel rengaModel,
            Geometry.Point3D Position,
            string text,
            TextObjectType textType = TextObjectType.ModelText,
            int LevelId = -1,
            double angleRadians = 0.0,
            TextOffsetMode mode = TextOffsetMode.NoTransform,
            TextSettingsConfig? textStyle = null)
        {
            if (PluginData.Project == null) return null;
            Renga.IOperation editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            Renga.INewEntityArgs creationParams = rengaModel.CreateNewEntityArgs();
            creationParams.TypeId = Renga.EntityTypes.ModelText;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            double x = 1 * cos - 0 * sin;
            double y = 1 * sin + 0 * cos;

            creationParams.Placement3D = new Renga.Placement3D()
            {
                Origin = new Renga.Point3D() { X = Position.X, Y = Position.Y, Z = Position.Z },
                xAxis = new Renga.Vector3D() { X = x, Y = y, Z = 0 },
                zAxis = new Renga.Vector3D() { X = 0, Y = 0, Z = 1 }
            };
            if (textType == TextObjectType.DrawingText)
            {
                creationParams.TypeId = Renga.EntityTypes.DrawingText;
                creationParams.Placement2D = new Renga.Placement2D()
                {
                    Origin = new Renga.Point2D() { X = Position.X, Y = Position.Y },
                    xAxis = new Renga.Vector2D() { X = x, Y = y }
                };
            }

            if (LevelId != -1)
            {
                //Пока прячем на 8.9. Это баг АПИ
                //if (PluginData.RengaVersion.CompareTo(new Version("8.9")) > 0) creationParams.HostObjectId = LevelId;
            }

            //Проверка уровня
            if (LevelId != -1 && textType == TextObjectType.ModelText)
            {
                Renga.IModelObject? levelObjectCheck = rengaModel.GetObjects().GetById(LevelId);
                if (levelObjectCheck != null && levelObjectCheck.ObjectType == RengaObjectTypes.Level) creationParams.HostObjectId = LevelId;
            }

            var textObjectRaw = rengaModel.CreateObject(creationParams);
            if (textObjectRaw == null)
            {
                editOperation.Rollback();
                return null;
            }
            Renga.ITextObject? textObject = textObjectRaw as Renga.ITextObject;
            if (textObject == null)
            {
                editOperation.Rollback();
                return null;
            }

            // Переходим к заданию текста для заданого стиля
            if (textStyle == null) textStyle = new TextSettingsConfig();
            Renga.IRichTextDocument textData = textObject.GetRichTextDocument();
            RichTextToken slopeInfo = new RichTextToken()
            {
                FontFamily = textStyle.FontName,
                FontCapSize = textStyle.FontCapSize,
                FontColor = textStyle.ToRengaFontColor(),
                FontStyle = textStyle.GetFontStyle(),
                Text = text
            };
            Array tokens = new RichTextToken[] { slopeInfo };
            IRichTextParagraph? textParagraph = textData.AppendParagraph(tokens);

            if (mode != TextOffsetMode.NoTransform)
            {
                var textRectangleSize = textObject.BoundRect;
                double x2 = 0, y2 = 0;
                if (mode == TextOffsetMode.Center)
                {
                    x2 = textRectangleSize.Top / 2 * cos - textRectangleSize.Right / 2 * sin;
                    y2 = textRectangleSize.Top / 2 * sin + textRectangleSize.Right / 2 * cos;
                }
                else if (mode == TextOffsetMode.TopLeftCorner)
                {
                    x2 = 0 * cos + textRectangleSize.Right * sin;
                    y2 = 0 * sin - textRectangleSize.Right * cos;
                }

                //У Renga в textRectangleSize заполнены только свойства Top и Right почему-то
                if (textType == TextObjectType.ModelText)
                {
                    Renga.ILevelObject? textObjectOnLevel = textObjectRaw as Renga.ILevelObject;
                    if (textObjectOnLevel != null)
                    {
                        var pl = textObjectOnLevel.GetPlacement();
                        //Сначала сдвигаем на середину ограничивающей рамки текста
                        pl.Move(new Vector3D()
                        {
                            X = x2,
                            Y = y2,
                            Z = 0
                        });
                        textObjectOnLevel.SetPlacement(pl);
                    }
                }
                else if (textType == TextObjectType.DrawingText)
                {
                    Renga.IPlacement2DObject? textObjectOnDrawing = textObjectRaw as Renga.IPlacement2DObject;
                    if (textObjectOnDrawing != null)
                    {
                        var pl = textObjectOnDrawing.GetPlacement();
                        //Сначала сдвигаем на середину ограничивающей рамки текста
                        pl.Move(new Vector2D()
                        {
                            X = x2,
                            Y = y2
                        });
                        textObjectOnDrawing.SetPlacement(pl);
                    }
                }
            }

            editOperation.Apply();

            Renga.IModelObject? textAsModelObject = textObject as Renga.IModelObject;
            return textAsModelObject;
        }

        public static Renga.IModelObject? CreateLevel(this Renga.IModel rengaModel, double elevation)
        {
            if (PluginData.Project == null) return null;
            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            Renga.INewEntityArgs creationParams = rengaModel.CreateNewEntityArgs();
            creationParams.TypeId = Renga.EntityTypes.Level;
            creationParams.Placement3D = new Renga.Placement3D()
            {
                //Тут не задает elevation
                Origin = new Renga.Point3D() { X = 0, Y = 0, Z = elevation },
                xAxis = new Renga.Vector3D() { X = 1, Y = 0, Z = 0 },
                zAxis = new Renga.Vector3D() { X = 0, Y = 0, Z = 1 }
            };

            var levelObjectRaw = rengaModel.CreateObject(creationParams);
            if (levelObjectRaw == null)
            {
                editOperation.Rollback();
                return null;
            }

            Renga.ILevel? levelObject = levelObjectRaw as Renga.ILevel;
            if (levelObject == null)
            {
                editOperation.Rollback();
                return null;
            }

            //Тут тоже не задает elevation
            levelObject.Placement.Move(new Vector3D() { X = 0, Y = 0, Z = elevation });
            editOperation.Apply();

            Renga.IModelObject? levelAsModelObject = levelObject as Renga.IModelObject;
            return levelAsModelObject;
        }

        public static Renga.IModelObject? CreatePositionObject(this Renga.IModel rengaModel, SinglePositionObjectType objectType, RengaBri4kaKernel.Geometry.Point3D position, int LevelId = -1, bool isMeters = true)
        {
            if (PluginData.Project == null) return null;

            Renga.IOperation editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            Renga.INewEntityArgs creationParams = rengaModel.CreateNewEntityArgs();
            //Проверка уровня
            if (LevelId != -1)
            {
                Renga.IModelObject? levelObjectCheck = rengaModel.GetObjects().GetById(LevelId);
                if (levelObjectCheck != null && levelObjectCheck.ObjectType == RengaObjectTypes.Level) creationParams.HostObjectId = LevelId;
            }

            switch (objectType)
            {
                case SinglePositionObjectType.Equipment: creationParams.TypeId = RengaObjectTypes.Equipment; break;
                case SinglePositionObjectType.Column: creationParams.TypeId = RengaObjectTypes.Column; break;
            }

            double numKoeff = 1.0;
            if (isMeters) numKoeff = 1000.0;

            creationParams.Placement3D = new Placement3D()
            {
                Origin = new Renga.Point3D()
                {
                    X = position.X * numKoeff,
                    Y = position.Y * numKoeff,
                    Z = position.Z * numKoeff,
                },
                xAxis = new Renga.Vector3D() { X = 1, Y = 0, Z = 0 },
                zAxis = new Renga.Vector3D() { X = 0, Y = 0, Z = 1 }
            };

            var createdRengaObject = rengaModel.CreateObject(creationParams);
            if (createdRengaObject == null)
            {
                editOperation.Rollback();
                return null;
            }

            editOperation.Apply();

            return createdRengaObject;
        }

        public static Renga.IModelObject? CreateBaselineObject(this Renga.IModel rengaModel, BaselineObjectType objectType, List<RengaBri4kaKernel.Geometry.Point3D> contourRaw, int LevelId = -1, bool isMeters = true)
        {
            if (PluginData.Project == null) return null;
            if (contourRaw.Count() < 2) return null;

            List<RengaBri4kaKernel.Geometry.Point3D> contour = contourRaw;
            //if (contourRaw[0] != contourRaw[contourRaw.Count - 1]) contour.Add(contourRaw[0]);

            Renga.IOperation editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            double numKoeff = 1.0;
            if (isMeters) numKoeff = 1000.0;

            Renga.INewEntityArgs creationParams = rengaModel.CreateNewEntityArgs();

            //Проверка уровня
            if (LevelId != -1)
            {
                Renga.IModelObject? levelObjectCheck = rengaModel.GetObjects().GetById(LevelId);
                if (levelObjectCheck != null && levelObjectCheck.ObjectType == RengaObjectTypes.Level) creationParams.HostObjectId = LevelId;
            }

            switch (objectType)
            {
                case BaselineObjectType.Hatch: creationParams.TypeId = RengaObjectTypes.Hatch; break;
                case BaselineObjectType.DrawingLine: creationParams.TypeId = RengaObjectTypes.DrawingLine; break;
                case BaselineObjectType.Line3d: creationParams.TypeId = RengaObjectTypes.Line3D; break;
                case BaselineObjectType.Roof: creationParams.TypeId = RengaObjectTypes.Roof; break;
                case BaselineObjectType.Beam: creationParams.TypeId = RengaObjectTypes.Beam; break;
                case BaselineObjectType.Wall: creationParams.TypeId = RengaObjectTypes.Wall; break;
                case BaselineObjectType.Floor: creationParams.TypeId = RengaObjectTypes.Floor; break;
            }

            var baseLineEntity = rengaModel.CreateObject(creationParams);
            if (baseLineEntity == null)
            {
                editOperation.Rollback();
                return null;
            }

            if (objectType == BaselineObjectType.Line3d | objectType == BaselineObjectType.Beam)
            {
                Renga.IBaseline3DObject? baseLineEntity_3D = baseLineEntity as Renga.IBaseline3DObject;
                if (baseLineEntity_3D == null)
                {
                    editOperation.Rollback();
                    return null;
                }

                List<Renga.ICurve3D> curvesTemp = new List<ICurve3D>();
                for (int vertexCounter = 0; vertexCounter < contour.Count - 1; vertexCounter++)
                {
                    var vertex1 = contour[vertexCounter];
                    var vertex2 = contour[vertexCounter + 1];

                    Renga.Point3D rengaVertex1 = new Renga.Point3D() { X = vertex1.X * numKoeff, Y = vertex1.Y * numKoeff, Z = vertex1.Z * numKoeff };
                    Renga.Point3D rengaVertex2 = new Renga.Point3D() { X = vertex2.X * numKoeff, Y = vertex2.Y * numKoeff, Z = vertex2.Z * numKoeff };
                    Renga.ICurve3D curveData = PluginData.rengaApplication.Math.CreateLineSegment3D(rengaVertex1, rengaVertex2);
                    curvesTemp.Add(curveData);
                }
                baseLineEntity_3D.SetBaseline(PluginData.rengaApplication.Math.CreateCompositeCurve3D(curvesTemp.ToArray()));
            }
            else
            {
                Renga.IBaseline2DObject? baseLineEntity_2D = baseLineEntity as Renga.IBaseline2DObject;
                if (baseLineEntity_2D == null)
                {
                    editOperation.Rollback();
                    return null;
                }

                if (contour[0] != contour[contour.Count - 1]) contour.Add(contour[0]);

                List<Renga.ICurve2D> curvesTemp = new List<ICurve2D>();
                for (int vertexCounter = 0; vertexCounter < contour.Count - 1; vertexCounter++)
                {
                    var vertex1 = contour[vertexCounter];
                    var vertex2 = contour[vertexCounter + 1];

                    Renga.Point2D rengaVertex1 = new Renga.Point2D() { X = vertex1.X * numKoeff, Y = vertex1.Y * numKoeff };
                    Renga.Point2D rengaVertex2 = new Renga.Point2D() { X = vertex2.X * numKoeff, Y = vertex2.Y * numKoeff };
                    Renga.ICurve2D curveData = PluginData.rengaApplication.Math.CreateLineSegment2D(rengaVertex1, rengaVertex2);
                    curvesTemp.Add(curveData);
                }

                baseLineEntity_2D.SetBaseline(PluginData.rengaApplication.Math.CreateCompositeCurve2D(curvesTemp.ToArray()));
            }

           
            //поднять объект

            Renga.ILevelObject? textObjectOnLevel = baseLineEntity as Renga.ILevelObject;
            if (textObjectOnLevel != null)
            {
                var pl = textObjectOnLevel.GetPlacement();
                //Сначала сдвигаем на середину ограничивающей рамки текста
                pl.Move(new Vector3D()
                {
                    X = 0,
                    Y = 0,
                    Z = contour[0].Z * numKoeff
                });
                textObjectOnLevel.SetPlacement(pl);
            }

            editOperation.Apply();
            return baseLineEntity;
        }

        public static Renga.ILevel? GetLevel(this Renga.IModel rengaModel, int id)
        {
            var rengaObjects = rengaModel.GetObjects();
            Renga.IModelObject? rengaObject = rengaObjects.GetById(id);
            if (rengaObject == null) return null;
            Renga.ILevel? rengaLevelObject = rengaObject as Renga.ILevel;
            return rengaLevelObject;

        }

        public static IEnumerable<Renga.IModelObject>? GetVisibleObjects()
        {
            var view = PluginData.rengaApplication.ActiveView;
            var modelView = view as Renga.IModelView;
            if (modelView == null) return null;

            Renga.IModel model = PluginData.Project.Model;
            Renga.IModelObjectCollection objects = model.GetObjects();
            List<Renga.IModelObject> needObjects = new List<IModelObject>();

            for (int i = 0; i < objects.Count; i++)
            {
                Renga.IModelObject o = objects.GetByIndex(i);
                if (modelView.IsObjectVisible(o.Id)) needObjects.Add(o);
            }

            return needObjects;
        }

        public static Renga.IModelObjectCollection? GetObjects(this Renga.IModel rengaModel)
        {
            Renga.IModelObjectCollection? result = null;
            try
            {
                result = rengaModel.GetObjects();
            }
            catch (Exception ex) { }
            return result;
        }

        public static int GetLevelIdByName(this Renga.IModel rengaModel, string name)
        {
            Renga.IModelObjectCollection objects = rengaModel.GetObjects();

            for (int i = 0; i < objects.Count; i++)
            {
                Renga.IModelObject o = objects.GetByIndex(i);
                if (o.ObjectType == RengaObjectTypes.Level)
                {
                    Renga.ILevel? oAsLevel = o.GetInterfaceByName("ILevel") as Renga.ILevel;
                    if (oAsLevel != null && oAsLevel.LevelName == name) return o.Id;
                }
                
            }
            return -1;
        }
    }
}

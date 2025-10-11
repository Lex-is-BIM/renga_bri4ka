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
        Floor
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
            double[] Position,
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
            creationParams.TypeId = Renga.ObjectTypes.ModelText;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            double x = 1 * cos - 0 * sin;
            double y = 1 * sin + 0 * cos;

            creationParams.Placement3D = new Renga.Placement3D()
            {
                Origin = new Renga.Point3D() { X = Position[0], Y = Position[1], Z = Position[2] },
                xAxis = new Renga.Vector3D() { X = x, Y = y, Z = 0 },
                zAxis = new Renga.Vector3D() { X = 0, Y = 0, Z = 1}
            };
            if (textType == TextObjectType.DrawingText)
            {
                creationParams.TypeId = Renga.ObjectTypes.DrawingText;
                creationParams.Placement2D = new Renga.Placement2D()
                {
                    Origin = new Renga.Point2D() { X = Position[0], Y = Position[1]},
                    xAxis = new Renga.Vector2D() { X = x, Y = y }
                };
            }

            if (LevelId != -1 && textType == TextObjectType.ModelText)
            {
                //Пока прячем на 8.9. Это баг АПИ
                if (PluginData.RengaVersion.CompareTo(new Version("8.9")) > 0) creationParams.HostObjectId = LevelId;
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
            creationParams.TypeId = Renga.ObjectTypes.Level;
            creationParams.Placement3D = new Renga.Placement3D()
            {
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
            levelObject.Placement.Move(new Vector3D() { X = 0, Y = 0, Z = elevation });
            editOperation.Apply();

            Renga.IModelObject? levelAsModelObject = levelObject as Renga.IModelObject;
            return levelAsModelObject;
        }

        public static Renga.IModelObject? CreateBaselineObject(this Renga.IModel rengaModel, BaselineObjectType objectType, List<RengaBri4kaKernel.Geometry.Point3D> contour, bool isMeters = true)
        {
            if (PluginData.Project == null) return null;
            if (contour.Count() < 3) return null;
            Renga.IOperation editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            double numKoeff = 1.0;
            if (isMeters) numKoeff = 1000.0;

            Renga.INewEntityArgs creationParams = rengaModel.CreateNewEntityArgs();
            if (objectType == BaselineObjectType.Hatch) creationParams.TypeId = Renga.ObjectTypes.Hatch;
            else if (objectType == BaselineObjectType.Line3d) creationParams.TypeId = Renga.ObjectTypes.Line3D;
            else if (objectType == BaselineObjectType.Floor) creationParams.TypeId = Renga.ObjectTypes.Floor;


            var hatchObjectRaw = rengaModel.CreateObject(creationParams);
            if (hatchObjectRaw == null)
            {
                editOperation.Rollback();
                return null;
            }

            Renga.IBaseline2DObject? hatchAsBaseline2dObject = hatchObjectRaw as Renga.IBaseline2DObject;
            if (hatchAsBaseline2dObject == null)
            {
                editOperation.Rollback();
                return null;
            }

            if (contour[0] != contour[contour.Count - 1]) contour.Add(contour[0]);

            List<Renga.ICurve2D> curvesTemp = new List<ICurve2D>();
            for (int vertexCounter = 0;  vertexCounter < contour.Count - 1; vertexCounter++)
            {
                var vertex1 = contour[vertexCounter];
                var vertex2 = contour[vertexCounter + 1];

                Renga.Point2D rengaVertex1 = new Renga.Point2D() { X = vertex1.X * numKoeff, Y = vertex1.Y * numKoeff };
                Renga.Point2D rengaVertex2 = new Renga.Point2D() { X = vertex2.X * numKoeff, Y = vertex2.Y * numKoeff };
                Renga.ICurve2D curveData = PluginData.rengaApplication.Math.CreateLineSegment2D(rengaVertex1, rengaVertex2);
                curvesTemp.Add(curveData);
            }

            hatchAsBaseline2dObject.SetBaseline(PluginData.rengaApplication.Math.CreateCompositeCurve2D(curvesTemp.ToArray()));
            editOperation.Apply();

            return hatchObjectRaw;
        }

        public static Renga.ILevel? GetLevel(this Renga.IModel rengaModel, int id)
        {
            var rengaObjects = rengaModel.GetObjects();
            Renga.IModelObject? rengaObject = rengaObjects.GetById(id);
            if (rengaObject == null) return null;
            Renga.ILevel? rengaLevelObject = rengaObject as Renga.ILevel;
            return rengaLevelObject;

        }
    }
}

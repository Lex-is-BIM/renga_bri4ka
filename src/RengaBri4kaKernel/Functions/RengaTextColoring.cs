using Renga;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.Extensions;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Functions
{
    internal class RengaTextColoring
    {
        public class ParametersTextColoring : PLuginParametersCollection
        {
            public static Guid TextColorId = new Guid("{7a7429dc-85ca-4f5a-8665-1de4647d526e}");
            public const string TextColor = "Bri4ka. Код цвета RGBA";

        }

        public RengaTextColoring()
        {
            RengaPropertiesUtils.RegisterPropertyIfNotReg(ParametersTextColoring.TextColorId, ParametersTextColoring.TextColor, PropertyType.PropertyType_String);

            RengaPropertiesUtils.AssignPropertiesToTypes(ParametersTextColoring.TextColorId, new Guid[] {RengaObjectTypes.ModelText, RengaObjectTypes.DrawingText});
        }

        public void SetColor(System.Windows.Media.Color color)
        {
            int[]? ids = UserInput.GetSelectedObjects();
            if (ids == null) return;

            var editOperation = PluginData.Project.CreateOperation();
            editOperation.Start();

            Renga.Color rengaColor = new Renga.Color() { Red = color.R, Green = color.G, Blue = color.B, Alpha = color.A };
            string colorStr = TextSettingsConfig.ToHex(color);
            Guid[] propsId = new Guid[] { ParametersTextColoring.TextColorId };

            var rengaModel = PluginData.GetModel();
            if (rengaModel == null) return;

            Renga.IModelObjectCollection modelObjects = rengaModel.GetObjects();

            foreach (int id in ids)
            {
                Renga.IModelObject rengaObject = modelObjects.GetById(id);
                Renga.ITextObject? textObject = rengaObject as Renga.ITextObject;
                if (textObject == null) continue;
                Renga.IRichTextDocument textDocument = textObject.GetRichTextDocument();

                for (int paragraphIdx = 0; paragraphIdx < textDocument.ParagraphCount; ++paragraphIdx)
                {
                    var paragraph = textDocument.GetParagraph(paragraphIdx);
                    var tokenCount = paragraph.TokenCount;
                    for (int tokenIdx = 0; tokenIdx < tokenCount; ++tokenIdx)
                    {
                        var oldToken = paragraph.GetToken(tokenIdx);
                        Renga.RichTextToken newToken = new Renga.RichTextToken()
                        {
                            Text = oldToken.Text,
                            FontFamily = oldToken.FontFamily,
                            FontCapSize = oldToken.FontCapSize,
                            FontColor = rengaColor,
                            FontStyle = oldToken.FontStyle,
                        };

                        paragraph.AppendToken(newToken);
                        paragraph.RemoveToken(tokenIdx);
                    }
                }

                rengaObject.SetObjectsProperties(propsId, new object[] { colorStr });
            }

            editOperation.Apply();
        }
    }
}

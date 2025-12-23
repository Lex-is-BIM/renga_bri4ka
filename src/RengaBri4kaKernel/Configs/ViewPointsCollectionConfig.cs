using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RengaBri4kaKernel.Configs
{
    public class ViewPointCameraParameters
    {
        public float[] Position { get; set; }
        public float[] FocusPoint { get; set; }
        public float[] UpVector { get; set; }

        public ViewPointCameraParameters()
        {
            Position = new float[] { 0f, 0f, 0f, };
            FocusPoint = new float[] { 0f, -1f, 0f};
            UpVector = new float[] { 0f, 0f, 1f, };
        }

        public override string ToString()
        {
            return $"Параметры камеры: \nPosition={string.Join(";", Position)}\nFocusPoint={string.Join(";", FocusPoint)}\nUpVector={string.Join(";", UpVector)}";
        }
    }

    public class ViewPointDefinition
    {
        [XmlIgnore]
        public const string NameBase = "ViewPoint";

        public Guid Id { get; set; }
        public string Name { get; set; }
        //public bool ShowOnlyVisibleEntities { get; set; }
        public int[] VisibleObjects { get; set; }

        public ViewPointCameraParameters Camera { get; set; }

        public Renga.VisualStyle ActiveStyle { get; set; }

        public ViewPointDefinition()
        {
            Id = Guid.NewGuid();
            Name = NameBase;
            ActiveStyle = Renga.VisualStyle.VisualStyle_Color;
            //ShowOnlyVisibleEntities = false;
            VisibleObjects = new int[] { };
            Camera = new ViewPointCameraParameters();
        }
    }
    public class ViewPointsCollectionConfig
    {
        public Guid TargetProjectId { get; set; }
        public List<ViewPointDefinition> ViewPoints { get; set; }

        public ViewPointsCollectionConfig()
        {
            TargetProjectId = Guid.Empty;
            ViewPoints = new List<ViewPointDefinition>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Configs
{

    public enum RoomGeometryVariant
    {
        Centroid,
        SolidsFloorContour,
        BaselineContour
    }

    public class LinkRoomWithFloorConfig : ConfigIO
    {
        public RoomGeometryVariant RoomGeometryMode { get; set; } = RoomGeometryVariant.Centroid;
        public bool UseOnlyVisible { get; set; } = true;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.RengaInternalResources
{
    internal enum RoofGridType : int
    {
        Bottom = 2,
        Cut = 7,
        OpeningBottom = 5,
        OpeningSide = 4,
        OpeningTop = 6,
        Side = 1,
        Top = 3,
        Undefined = 0
    }


    internal enum RampGridTypes : int
    {
        BackSide = 4,
        Bottom = 2,
        Cut = 7,
        FrontSide = 3,
        LeftSide = 5,
        RightSide = 6,
        Top = 1,
        Undefined = 0
    }
}

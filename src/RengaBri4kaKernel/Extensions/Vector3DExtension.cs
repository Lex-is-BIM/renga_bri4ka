using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{
    internal static class Vector3DExtension
    {
        public static Renga.Vector3D OX
        {
            get
            {
                return new Renga.Vector3D() { X = 1, Y = 0, Z = 0 };
            }
        }
    }
}

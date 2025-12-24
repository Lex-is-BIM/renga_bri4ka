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

        public static Renga.Vector3D OY
        {
            get
            {
                return new Renga.Vector3D() { X = 0, Y = 1, Z = 0 };
            }
        }

        public static Renga.Vector3D OZ
        {
            get
            {
                return new Renga.Vector3D() { X = 0, Y = 0, Z = 1 };
            }
        }
    }
}

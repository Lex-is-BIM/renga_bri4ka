using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.Extensions
{
    internal static class RoomExtension
    {
        public static Line3D? GetSolidsExternalContour(this Renga.IModelObject rengaRoomObject)
        {
            Renga.IExportedObject3D? rengaSolidGeometry = rengaRoomObject.GetExportedObject3D();
            if (rengaSolidGeometry == null) return null;

            Renga.IMesh rengaSolidGeometry_Mesh = rengaSolidGeometry.GetMesh(0);
            Renga.IGrid? rengaSolidGeometry_GridFloor = null;
            for (int rengaGridCounter = 0; rengaGridCounter < rengaSolidGeometry_Mesh.GridCount; rengaGridCounter++)
            {
                Renga.IGrid grid = rengaSolidGeometry_Mesh.GetGrid(rengaGridCounter);
                if (grid.GridType == (int)Renga.GridTypes.Room.Floor)
                {
                    rengaSolidGeometry_GridFloor = grid;
                    break;
                }
            }

            if (rengaSolidGeometry_GridFloor != null) return rengaSolidGeometry_GridFloor.ExtractExternalContour();
            return null;
        }

        public static Point3D? GetCentroid(this Renga.IModelObject rengaRoomObject)
        {
            Line3D? roomLineGeometry = rengaRoomObject.GetLineGeometry();
            if (roomLineGeometry == null) return null;

            return roomLineGeometry.GetCentroid();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RengaBri4kaKernel.Configs;

namespace RengaBri4kaKernel.Extensions
{
    public static class Camera3DExtension
    {
        public static ViewPointCameraParameters GetCameraParameters(this Renga.ICamera3D rengaCamera)
        {
            ViewPointCameraParameters cameraInfo = new ViewPointCameraParameters();
            cameraInfo.Position = new float[] { rengaCamera.Position.X, rengaCamera.Position.Y, rengaCamera.Position.Z };
            cameraInfo.FocusPoint = new float[] { rengaCamera.FocusPoint.X, rengaCamera.FocusPoint.Y, rengaCamera.FocusPoint.Z };
            cameraInfo.UpVector = new float[] { rengaCamera.UpVector.X, rengaCamera.UpVector.Y, rengaCamera.UpVector.Z };

            return cameraInfo;
        }
        public static void SetCameraParameters(this Renga.ICamera3D rengaCamera, ViewPointCameraParameters cameraParams)
        {
            Renga.FloatPoint3D focusPoint = new Renga.FloatPoint3D();
            focusPoint.X = cameraParams.FocusPoint[0];
            focusPoint.Y = cameraParams.FocusPoint[1];
            focusPoint.Z = cameraParams.FocusPoint[2];

            Renga.FloatPoint3D position = new Renga.FloatPoint3D();
            position.X = cameraParams.Position[0];
            position.Y = cameraParams.Position[1];
            position.Z = cameraParams.Position[2];

            Renga.FloatVector3D upVector = new Renga.FloatVector3D();
            upVector.X = cameraParams.UpVector[0];
            upVector.Y = cameraParams.UpVector[1];
            upVector.Z = cameraParams.UpVector[2];

            rengaCamera.LookAt(focusPoint, position, upVector);
        }
    }
}

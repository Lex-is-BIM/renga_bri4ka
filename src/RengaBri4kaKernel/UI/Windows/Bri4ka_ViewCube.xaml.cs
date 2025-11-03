using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using RengaBri4kaKernel.Functions;

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_ViewCube.xaml
    /// </summary>
    public partial class Bri4ka_ViewCube : Window
    {
        private Model3DGroup cubeModel;
        private AxisAngleRotation3D cameraRotation;
        private double cameraDistance = 10;
        private bool sceneUpdateStatus = false;
        private readonly RengaCameraHandlerViewCube2 _service;

        public Bri4ka_ViewCube()
        {
            InitializeComponent();
            CreateViewCubeWithAxis();

            _service = new RengaCameraHandlerViewCube2(UpdateCameraInfo);
        }

        private void UpdateCameraInfo(RengaCameraInfo value)
        {
            // Use Dispatcher since this might be called from a different thread
            Dispatcher.Invoke(() =>
            {
                this.CameraPositionText.Text = value.Info;
                AnimateCameraTo2(null, value.FocusPoint, value.UpVector);

            });
        }

        private void CreateViewCubeWithAxis()
        {
            cubeModel = new Model3DGroup();

            // Create cube faces with different colors
            CreateCubeFace(new Point3D(-1, -1, 1), new Point3D(1, -1, 1), new Point3D(1, 1, 1), new Point3D(-1, 1, 1), Colors.Red, "Front");    // Front
            CreateCubeFace(new Point3D(1, -1, -1), new Point3D(-1, -1, -1), new Point3D(-1, 1, -1), new Point3D(1, 1, -1), Colors.Green, "Back");   // Back
            CreateCubeFace(new Point3D(-1, 1, -1), new Point3D(-1, 1, 1), new Point3D(1, 1, 1), new Point3D(1, 1, -1), Colors.Blue, "Top");     // Top
            CreateCubeFace(new Point3D(-1, -1, 1), new Point3D(-1, -1, -1), new Point3D(1, -1, -1), new Point3D(1, -1, 1), Colors.Yellow, "Bottom"); // Bottom
            CreateCubeFace(new Point3D(1, -1, 1), new Point3D(1, -1, -1), new Point3D(1, 1, -1), new Point3D(1, 1, 1), Colors.Orange, "Right");  // Right
            CreateCubeFace(new Point3D(-1, -1, -1), new Point3D(-1, -1, 1), new Point3D(-1, 1, 1), new Point3D(-1, 1, -1), Colors.Purple, "Left");  // Left

            //Create axis (XYZ)


            // Add lighting for the cube
            cubeModel.Children.Add(new AmbientLight(Colors.White));
            cubeModel.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)));

            ModelVisual3D cubeVisual = new ModelVisual3D();
            cubeVisual.Content = cubeModel;
            ViewCubeViewport.Children.Add(cubeVisual);
        }

        private void DrawCoordsAxis()
        {

        }

        private void CreateCubeFace(Point3D p1, Point3D p2, Point3D p3, Point3D p4, Color color, string faceName)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Add positions
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);
            mesh.Positions.Add(p4);

            // Add triangle indices (two triangles per face)
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            GeometryModel3D face = new GeometryModel3D();
            face.Geometry = mesh;
            face.Material = new DiffuseMaterial(new SolidColorBrush(color));

            // Add mouse event handling
            ModelUIElement3D uiElement = new ModelUIElement3D();
            uiElement.Model = face;

            ViewCubeViewport.Children.Add(uiElement);
        }


        private void AnimateCameraTo(Point3D? lookPosition, Vector3D lookDirection, Vector3D upDirection)
        {
            //lookDirection = new Vector3D(
            //    lookDirection.X - lookPosition.X,
            //    lookDirection.Y - lookPosition.Y,
            //    lookDirection.Z - lookPosition.Z);

            //lookPosition = new Point3D(5, 5, 5);
            //lookPosition = GetPositionFromFocusPointAndUpVector(lookDirection, upDirection);

            if (!lookPosition.HasValue)
            {
                lookPosition = GetPositionFromFocusPointAndUpVector(lookDirection, upDirection);
            }

            // Also update the view cube camera to match
            CubeCamera.LookDirection = lookDirection;
            CubeCamera.UpDirection = upDirection;
            CubeCamera.Position = lookPosition.Value;

            //RengaBri4kaKernel.AuxFunctions.RengaUtils.LookTo(new double[] { v.X, v.Y, v.Z }, new double[] { lookDirection.X, lookDirection.Y, lookDirection.Z }, new double[] { upDirection.X, upDirection.Y, upDirection.Z });
        }

        private void AnimateCameraTo2(double[]? position, double[] lookDirection, double[] upDirection)
        {
            AnimateCameraTo(
                null,
                new Vector3D(lookDirection[0], lookDirection[1], lookDirection[2]),
                new Vector3D(upDirection[0], upDirection[1], upDirection[2]));
            //new Point3D(position[0], position[1], position[2])
        }

        private Point3D GetPositionFromFocusPointAndUpVector(Vector3D focusDir, Vector3D upVector)
        {
            var p = -focusDir * cameraDistance;
            return new Point3D(p.X, p.Y, p.Z);
        }

        private void ResetView_Click(object sender, RoutedEventArgs e)
        {
            var f = new Vector3D(-1, -1, -1);
            var up = new Vector3D(0, 0, 1);
            AnimateCameraTo(GetPositionFromFocusPointAndUpVector(f, up), f, up);
        }
    }
}

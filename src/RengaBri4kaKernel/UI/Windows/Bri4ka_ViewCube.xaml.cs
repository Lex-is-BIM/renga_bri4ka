using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
            DrawViewCube();
            DrawCoordinateAxes();

            _service = new RengaCameraHandlerViewCube2(UpdateCameraInfo);
        }

        private void UpdateCameraInfo(RengaCameraInfo value)
        {
            // Use Dispatcher since this might be called from a different thread
            Dispatcher.Invoke(() =>
            {
                this.RengaCamera_Info.Text = value.Info;
                AnimateCameraTo2(value.Position, value.FocusPoint, value.UpVector);

            });
        }

        private void DrawViewCube()
        {
            cubeModel = new Model3DGroup();

            // Create cube faces with different colors
            CreateCubeFace(new Point3D(-1, -1, 1), new Point3D(1, -1, 1), new Point3D(1, 1, 1), new Point3D(-1, 1, 1), Colors.Blue, "Front");    // Front
            CreateCubeFace(new Point3D(1, -1, -1), new Point3D(-1, -1, -1), new Point3D(-1, 1, -1), new Point3D(1, 1, -1), Colors.Blue, "Back");   // Back
            CreateCubeFace(new Point3D(-1, 1, -1), new Point3D(-1, 1, 1), new Point3D(1, 1, 1), new Point3D(1, 1, -1), Colors.Green, "Top");     // Top
            CreateCubeFace(new Point3D(-1, -1, 1), new Point3D(-1, -1, -1), new Point3D(1, -1, -1), new Point3D(1, -1, 1), Colors.Green, "Bottom"); // Bottom
            CreateCubeFace(new Point3D(1, -1, 1), new Point3D(1, -1, -1), new Point3D(1, 1, -1), new Point3D(1, 1, 1), Colors.Red, "Right");  // Right
            CreateCubeFace(new Point3D(-1, -1, -1), new Point3D(-1, -1, 1), new Point3D(-1, 1, 1), new Point3D(-1, 1, -1), Colors.Red, "Left");  // Left

            // Add lighting for the cube
            cubeModel.Children.Add(new AmbientLight(Colors.White));
            cubeModel.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)));

            ModelVisual3D cubeVisual = new ModelVisual3D();
            cubeVisual.Content = cubeModel;
            ViewCubeViewport.Children.Add(cubeVisual);
        }

        private void DrawCoordinateAxes()
        {
            Model3DGroup axesGroup = new Model3DGroup();
            double axisLength = 4;
            // X Axis (Red)
            DrawAxis(axesGroup, new Point3D(0, 0, 0), new Point3D(axisLength, 0, 0), Colors.Red, "X");
            // Y Axis (Green)
            DrawAxis(axesGroup, new Point3D(0, 0, 0), new Point3D(0, axisLength, 0), Colors.Green, "Y");
            // Z Axis (Blue)
            DrawAxis(axesGroup, new Point3D(0, 0, 0), new Point3D(0, 0, axisLength), Colors.Blue, "Z");

            ModelVisual3D axesVisual = new ModelVisual3D();
            axesVisual.Content = axesGroup;
            ViewCubeViewport.Children.Add(axesVisual);
        }

        private void DrawAxis(Model3DGroup group, Point3D start, Point3D end, Color color, string label)
        {
            // Draw the axis line
            MeshGeometry3D lineMesh = CreateCylinderMesh(start, end, 0.03);
            GeometryModel3D lineModel = new GeometryModel3D();
            lineModel.Geometry = lineMesh;
            lineModel.Material = new DiffuseMaterial(new SolidColorBrush(color));
            group.Children.Add(lineModel);

            // Draw arrowhead if enabled
            //if (ShowArrowsCheckBox.IsChecked == true)
            {
                Vector3D direction = Point3D.Subtract(end, start);
                direction.Normalize();

                double arrowheadLength = 0.3;
                double arrowheadRadius = 0.08;

                Point3D arrowBase = Point3D.Subtract(end, direction * arrowheadLength);
                MeshGeometry3D arrowMesh = CreateConeMesh(arrowBase, end, arrowheadRadius);
                GeometryModel3D arrowModel = new GeometryModel3D();
                arrowModel.Geometry = arrowMesh;
                arrowModel.Material = new DiffuseMaterial(new SolidColorBrush(color));
                group.Children.Add(arrowModel);
            }

            // Add label if enabled
            AddAxisLabel(group, end, color, label);
        }

        private MeshGeometry3D CreateCylinderMesh(Point3D start, Point3D end, double radius)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            Vector3D direction = Point3D.Subtract(end, start);
            double length = direction.Length;
            direction.Normalize();

            // Find an arbitrary perpendicular vector
            Vector3D perpendicular = GetPerpendicularVector(direction);
            Vector3D cross = Vector3D.CrossProduct(direction, perpendicular);

            int segments = 8;
            double angleStep = 2 * Math.PI / segments;

            // Create vertices for start and end circles
            for (int i = 0; i < segments; i++)
            {
                double angle = i * angleStep;
                double nextAngle = (i + 1) * angleStep;

                Vector3D offset1 = perpendicular * Math.Cos(angle) + cross * Math.Sin(angle);
                Vector3D offset2 = perpendicular * Math.Cos(nextAngle) + cross * Math.Sin(nextAngle);

                Point3D p1 = start + offset1 * radius;
                Point3D p2 = start + offset2 * radius;
                Point3D p3 = end + offset2 * radius;
                Point3D p4 = end + offset1 * radius;

                // Add two triangles for this segment
                AddTriangle(mesh, p1, p2, p3);
                AddTriangle(mesh, p1, p3, p4);
            }

            return mesh;
        }

        private MeshGeometry3D CreateConeMesh(Point3D baseCenter, Point3D tip, double baseRadius)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            Vector3D direction = Point3D.Subtract(tip, baseCenter);
            double height = direction.Length;
            direction.Normalize();

            Vector3D perpendicular = GetPerpendicularVector(direction);
            Vector3D cross = Vector3D.CrossProduct(direction, perpendicular);

            int segments = 8;
            double angleStep = 2 * Math.PI / segments;

            for (int i = 0; i < segments; i++)
            {
                double angle = i * angleStep;
                double nextAngle = (i + 1) * angleStep;

                Vector3D offset1 = perpendicular * Math.Cos(angle) + cross * Math.Sin(angle);
                Vector3D offset2 = perpendicular * Math.Cos(nextAngle) + cross * Math.Sin(nextAngle);

                Point3D base1 = baseCenter + offset1 * baseRadius;
                Point3D base2 = baseCenter + offset2 * baseRadius;

                // Add triangle from base to tip
                AddTriangle(mesh, base1, base2, tip);

                // Add base triangle (optional - for closed cone base)
                AddTriangle(mesh, baseCenter, base2, base1);
            }

            return mesh;
        }

        private Vector3D GetPerpendicularVector(Vector3D vector)
        {
            // Find a vector perpendicular to the input vector
            if (Math.Abs(vector.X) < Math.Abs(vector.Y))
            {
                return Math.Abs(vector.X) < Math.Abs(vector.Z)
                    ? new Vector3D(1, 0, 0)
                    : new Vector3D(0, 0, 1);
            }
            else
            {
                return Math.Abs(vector.Y) < Math.Abs(vector.Z)
                    ? new Vector3D(0, 1, 0)
                    : new Vector3D(0, 0, 1);
            }
        }


        private void AddAxisLabel(Model3DGroup group, Point3D position, Color color, string text)
        {
            // Create a simple billboard label using GeometryModel3D
            // For a more advanced solution, you might want to use 2D visuals on top of 3D

            // Offset the label slightly from the axis end
            Point3D labelPosition = new Point3D(
                position.X + 0.2,
                position.Y + 0.2,
                position.Z + 0.2
            );

            // Create a simple quad for the label background
            MeshGeometry3D labelMesh = new MeshGeometry3D();

            double size = 0.3;
            Point3D[] corners = {
                new Point3D(labelPosition.X - size, labelPosition.Y - size, labelPosition.Z),
                new Point3D(labelPosition.X + size, labelPosition.Y - size, labelPosition.Z),
                new Point3D(labelPosition.X + size, labelPosition.Y + size, labelPosition.Z),
                new Point3D(labelPosition.X - size, labelPosition.Y + size, labelPosition.Z)
            };

            AddTriangle(labelMesh, corners[0], corners[1], corners[2]);
            AddTriangle(labelMesh, corners[0], corners[2], corners[3]);

            GeometryModel3D labelModel = new GeometryModel3D();
            labelModel.Geometry = labelMesh;
            labelModel.Material = new DiffuseMaterial(new SolidColorBrush(Colors.White));

            group.Children.Add(labelModel);
        }

        private void AddTriangle(MeshGeometry3D mesh, Point3D p1, Point3D p2, Point3D p3)
        {
            int index = mesh.Positions.Count;
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);
            mesh.TriangleIndices.Add(index);
            mesh.TriangleIndices.Add(index + 1);
            mesh.TriangleIndices.Add(index + 2);
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


        private void AnimateCameraTo(double[] lookPositionRaw, double[] lookDirectionRaw, double[] upDirectionRaw)
        {
            CubeCamera.Position = new Point3D(lookPositionRaw[0], lookPositionRaw[1], lookPositionRaw[2]);
            CubeCamera.LookDirection = new Vector3D(lookDirectionRaw[0], lookDirectionRaw[1], lookDirectionRaw[2]);
            CubeCamera.UpDirection = new Vector3D(upDirectionRaw[0], upDirectionRaw[1], upDirectionRaw[2]);

            // Упрощаем координаты для записи в форму
            this.ViewCube_CameraInfo.Text =
                $"Position: {lookPositionRaw[0].ToString("0.##")};{lookPositionRaw[1].ToString("0.##")};{lookPositionRaw[2].ToString("0.##")}\n" +
                $"LookDirection: {lookDirectionRaw[0].ToString("0.##")};{lookDirectionRaw[1].ToString("0.##")};{lookDirectionRaw[2].ToString("0.##")}\n" +
                $"UpDirection: {upDirectionRaw[0].ToString("0.##")};{upDirectionRaw[1].ToString("0.##")};{upDirectionRaw[2].ToString("0.##")}";

            //RengaBri4kaKernel.AuxFunctions.RengaUtils.LookTo(new double[] { v.X, v.Y, v.Z }, new double[] { lookDirection.X, lookDirection.Y, lookDirection.Z }, new double[] { upDirection.X, upDirection.Y, upDirection.Z });
        }

        private void AnimateCameraTo2(double[] position, double[] focusPoint, double[] upDirection)
        {
            // Вектор направления взгляда считатся на основе точки фокуса (renga) и точки положения камеры (renga)
            double[] lookDirection = new double[]
            {
                -focusPoint[0] + position[0],
                -focusPoint[1] + position[1],
                -focusPoint[2] + position[2]
            };

            position = lookDirection;

            // Положение камеры нужно пересчитать. 
            // Положение камеры нужно сдвинуть для видимости куба с расстояния cameraDistance метров
            // Найдем расстояние от текущего position до 0 коорддинат
            double positionLength = Math.Sqrt(position[0] * position[0] + position[1] * position[1] + position[2] * position[2]);

            // Найдем поправку к координатам как отношение высоты орбиты камеры к positionLength
            double positionLengthCoefficient = cameraDistance / positionLength / cameraDistance;

            // Применим поправку к координатам
            double[] lookPosition = new double[]
            {
                position[0] * positionLengthCoefficient,
                position[1] * positionLengthCoefficient,
                position[2] * positionLengthCoefficient,
            };

            lookDirection = lookPosition;


            AnimateCameraTo(
                lookDirection,
                GetPositionFromFocusPointAndUpVector(lookDirection),
                upDirection);

            /*
            AnimateCameraTo(
                lookPosition,
                lookDirection,
                upDirection);
            
            //Нужно уменьшить lookDirection
            lookDirection = new double[] { -lookDirection[0] + position[0], -lookDirection[1] + position[1], -lookDirection[2] + position[2] };

            if (lookDirection.Max() > cameraDistance)
            {
                double maxV = lookDirection.Max();
                lookDirection = new double[]
                {
                    lookDirection[0] - maxV,
                    lookDirection[1] - maxV,
                    lookDirection[2] - maxV,
                };
            }
            AnimateCameraTo(
                null,
                new Vector3D(lookDirection[0], lookDirection[1], lookDirection[2]),
                new Vector3D(upDirection[0], upDirection[1], upDirection[2]));
            //new Point3D(position[0], position[1], position[2])
            */
        }

        private double[] GetPositionFromFocusPointAndUpVector(double[] focusDir)
        {
            double[] p = new double[] {
                -focusDir[0] * cameraDistance,
                -focusDir[1] * cameraDistance,
                -focusDir[2] * cameraDistance};
            return p;
        }

        private void ResetView_Click(object sender, RoutedEventArgs e)
        {
            var f = new double[] { -0.1, -0.1, -0.1 };
            var up = new double[]{0, 0, 1};
            AnimateCameraTo(GetPositionFromFocusPointAndUpVector(f), f, up);
        }

        private void Button_SetOrientFix_Top_Click(object sender, RoutedEventArgs e)
        {
            var f = new double[] { 0, 0, 1 };
            var up = new double[] { 0, 0, 1 };
            AnimateCameraTo(GetPositionFromFocusPointAndUpVector(f), f, up);
        }

        private void Button_SetOrientFix_Down_Click(object sender, RoutedEventArgs e)
        {
            var f = new double[] { 0, 0, -1 };
            var up = new double[] { 0, 0, 1 };
            AnimateCameraTo(GetPositionFromFocusPointAndUpVector(f), f, up);
        }
    }
}

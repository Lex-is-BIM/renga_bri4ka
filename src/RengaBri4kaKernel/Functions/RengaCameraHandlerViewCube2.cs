using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Functions
{
    internal struct CameraParameters
    {
        public double[] Position;
        public double[] Direction;
        public double[] UpVector;

        public string RengaCameraInfo;
        public string ViewcubeCameraInfo;
    }
    internal class RengaCameraHandlerViewCube2
    {
        private Timer _timer;
        private readonly object _lock = new object();
        private bool _disposed = false;
        private const double pRoundCoords = 1000; //mm

        private readonly Action<CameraParameters> _progressCallback;

        private const int pTimerIntervals = 1;
        public const double pCameraDistance = 10.0;

        public RengaCameraHandlerViewCube2(Action<CameraParameters> progressCallback)
        {
            _progressCallback = progressCallback;

            lock (_lock)
            {
                if (_disposed) return;

                _timer = new Timer(async _ => await ExecuteWorkAsync(),
                                  null,
                                  TimeSpan.Zero, // Start immediately
                                  TimeSpan.FromSeconds(pTimerIntervals)); // Repeat every pTimerIntervals second
            }
        }

        private async Task ExecuteWorkAsync()
        {
            try
            {
                // Your actual work goes here
                await DoWorkAsync();
            }
            catch (Exception ex)
            {
                // Handle exceptions if needed
                Console.WriteLine($"Error in background work: {ex.Message}");
            }
        }

        private async Task DoWorkAsync()
        {
            if (PluginData.rengaApplication != null)
            {
                //await Task.Delay(100);

                Renga.IView view = PluginData.rengaApplication.ActiveView as Renga.IView;
                if (view.Type == Renga.ViewType.ViewType_View3D)
                {
                    Renga.IModelView? viewModel = view as Renga.IModelView;
                    if (viewModel != null)
                    {
                        Renga.IView3DParams? viewModelParams = viewModel as Renga.IView3DParams;
                        if (viewModelParams != null)
                        {
                            Renga.ICamera3D camera = viewModelParams.Camera;
                            if (camera != null)
                            {
                                CameraParameters cameraInfo = CalculateParameters(
                                    new double[] {
                                        camera.Position.X / pRoundCoords,
                                        camera.Position.Y / pRoundCoords,
                                        camera.Position.Z / pRoundCoords },
                                    new double[] {
                                        camera.FocusPoint.X / pRoundCoords,
                                        camera.FocusPoint.Y / pRoundCoords,
                                        camera.FocusPoint.Z / pRoundCoords },
                                    new double[] {
                                        camera.UpVector.X,
                                        camera.UpVector.Y,
                                        camera.UpVector.Z },
                                    camera.FovVertical,
                                    camera.FovHorizontal
                                    );
                                
                                _progressCallback.Invoke(cameraInfo);
                            }
                        }
                    }
                }
            }
        }

        private CameraParameters CalculateParameters(double[] cameraOriginPosition, double[] cameraOriginFocusPoint, double[] cameraOriginUpVector, double FovVertical, double FovHorizontal)
        {
            // Вектор направления взгляда считатся на основе точки фокуса и точки положения камеры
            double[] lookDirection = new double[]
            {
                -cameraOriginFocusPoint[0] + cameraOriginPosition[0],
                -cameraOriginFocusPoint[1] + cameraOriginPosition[1],
                -cameraOriginFocusPoint[2] + cameraOriginPosition[2]
            };

            // Упростим вектор так, чтобы его составляющие X, Y, Z были в единицах cameraDistance
            double[] lookDirectionTmp = lookDirection.Select(c => Math.Abs(c)).ToArray();

            // Определим текущую длину вектора lookDirection
            double lookDirectionLength = Math.Sqrt(lookDirection[0] * lookDirection[0] + lookDirection[1] * lookDirection[1] + lookDirection[2] * lookDirection[2]);

            // Найдем поправку к координатам как отношение высоты орбиты камеры к positionLength
            // Маленькая поправка 1.1, чтобы секущей плоскостью не резалась геометрия куба и стрелок осей

            double lookDirectionCoefficient = pCameraDistance * 1.1 / lookDirectionLength;

            // Зададим новое значение координат вектора lookDirection
            lookDirection = new double[]
            {
                lookDirection[0] * lookDirectionCoefficient,
                lookDirection[1] * lookDirectionCoefficient,
                lookDirection[2] * lookDirectionCoefficient,
            };

            double[] direction = new double[]
            {
                -lookDirection[0] * pCameraDistance,
                -lookDirection[1] * pCameraDistance,
                -lookDirection[2] * pCameraDistance
            };

            CameraParameters parameters = new CameraParameters()
            {
                Position = lookDirection,
                Direction = direction,
                UpVector = cameraOriginUpVector,
                RengaCameraInfo =
                    $"Position: {string.Join(";", cameraOriginPosition.Select(c=>c.ToString("0.##")))}\n" +
                    $"FocusPoint: {string.Join(";", cameraOriginFocusPoint.Select(c => c.ToString("0.##")))}\n" +
                    $"UpVector: {string.Join(";", cameraOriginUpVector.Select(c => c.ToString("0.##")))}\n" +
                    $"FovVertical: {FovVertical}\n" +
                    $"FovHorizontal: {FovHorizontal}",
                ViewcubeCameraInfo =
                    $"Position: {string.Join(";", lookDirection.Select(c => c.ToString("0.##")))}\n" +
                    $"Direction: {string.Join(";", direction.Select(c => c.ToString("0.##")))}\n" +
                    $"UpVector: {string.Join(";", cameraOriginUpVector.Select(c => c.ToString("0.##")))}",
            };
            return parameters;
        }

        ~RengaCameraHandlerViewCube2()
        {
            lock (_lock)
            {
                _disposed = true;
                _timer?.Dispose();
            }
        }

    }
}

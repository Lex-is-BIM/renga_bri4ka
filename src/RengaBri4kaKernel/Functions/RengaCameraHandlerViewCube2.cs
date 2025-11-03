using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RengaBri4kaKernel.UI.Windows;

namespace RengaBri4kaKernel.Functions
{
    internal struct RengaCameraInfo
    {
        public double[] Position;
        public double[] FocusPoint;
        public double[] UpVector;

        public string Info;
    }
    internal class RengaCameraHandlerViewCube2
    {
        private Timer _timer;
        private readonly object _lock = new object();
        private bool _disposed = false;
        private const double pRoundCoords = 1000; //mm

        private readonly Action<RengaCameraInfo> _progressCallback;

        private const int pTimerIntervals = 2;

        public RengaCameraHandlerViewCube2(Action<RengaCameraInfo> progressCallback)
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
                                RengaCameraInfo cameraInfo = new RengaCameraInfo()
                                {
                                    Position = new double[] {
                                        camera.Position.X / pRoundCoords,
                                        camera.Position.Y / pRoundCoords,
                                        camera.Position.Z / pRoundCoords },
                                    FocusPoint = new double[] {
                                        camera.FocusPoint.X / pRoundCoords,
                                        camera.FocusPoint.Y / pRoundCoords,
                                        camera.FocusPoint.Z / pRoundCoords },
                                    UpVector = new double[] {
                                        camera.UpVector.X,
                                        camera.UpVector.Y,
                                        camera.UpVector.Z },
                                    Info =
                                    $"Position: {camera.Position.X};{camera.Position.Y};{camera.Position.Z}\n" +
                                    $"FocusPoint: {camera.FocusPoint.X};{camera.FocusPoint.Y};{camera.FocusPoint.Z}\n" +
                                    $"UpVector: {camera.UpVector.X};{camera.UpVector.Y};{camera.UpVector.Z}\n" +
                                    $"FovVertical: {camera.FovVertical}\n" +
                                    $"FovHorizontal: {camera.FovHorizontal}"
                                };
                                _progressCallback.Invoke(cameraInfo);
                            }
                        }
                    }
                }
            }
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

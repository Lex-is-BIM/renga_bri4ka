using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RengaBri4kaKernel.UI.Windows;

namespace RengaBri4kaKernel.Functions
{
    /// <summary>
    /// Вспомогательный класс-сервис, осуществляющий анализ камеры модели и визуализацию положения в виде видового куба на окне
    /// </summary>
    public class RengaCameraHandlerViewCube
    {
        private static RengaCameraHandlerViewCube mInstance;
        private Timer _timer;
        private readonly object _lock = new object();
        private bool _disposed = false;

        private Bri4ka_ViewCube? pControl;
        private bool pControlUpdateInfo = false;

        private const int pTimerIntervals = 2;

        public RengaCameraHandlerViewCube()
        {
            lock (_lock)
            {
                if (_disposed) return;

                _timer = new Timer(async _ => await ExecuteWorkAsync(),
                                  null,
                                  TimeSpan.Zero, // Start immediately
                                  TimeSpan.FromSeconds(pTimerIntervals)); // Repeat every pTimerIntervals second
            }
        }

        public void ShowViewCubeControl()
        {
            if (pControl == null)
            {
                pControl = new Bri4ka_ViewCube();
                pControl.Closing += PControl_Closing;
                pControl.Show();
                pControlUpdateInfo = true;
            }
            else
            {
                pControl = new Bri4ka_ViewCube();
                pControl.Closing += PControl_Closing;
                pControl.Show();
                pControlUpdateInfo = true;
            }
        }

        private void PControl_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            pControlUpdateInfo = false;
        }

        public static RengaCameraHandlerViewCube GetInstance()
        {
            if (mInstance == null) mInstance = new RengaCameraHandlerViewCube();
            return mInstance;
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
            if (PluginData.rengaApplication != null && pControl != null)
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
                                /*
                                pControl.SetCameraInfo(
                                    $"{camera.Position.X};{camera.Position.Y};{camera.Position.Z}",
                                    $"{camera.FocusPoint.X};{camera.FocusPoint.Y};{camera.FocusPoint.Z}",
                                    $"{camera.UpVector.X};{camera.UpVector.Y};{camera.UpVector.Z}",
                                    $"{camera.FovVertical}",
                                    $"{camera.FovHorizontal}"
                                    );
                                */
                            }
                        }
                    }
                }
            }
        }

        ~RengaCameraHandlerViewCube()
        {
            lock (_lock)
            {
                _disposed = true;
                _timer?.Dispose();
            }
        }
    }
}

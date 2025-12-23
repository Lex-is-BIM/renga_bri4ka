using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Extensions
{

    internal static class ApplicationExtension
    {
        public static bool CloseProject2(this Renga.IApplication application)
        {
            if (application.Project == null) return true;
            if (application.Project.HasActiveOperation()) return false;
            if (application.Project.HasUnsavedChanges()) application.Project.Save();
            application.CloseProject(false);

            return true;

        }

        public static Renga.ICamera3D? GetCamera(this Renga.IApplication application)
        {
            Renga.IView view = application.ActiveView as Renga.IView;
            if (view.Type != Renga.ViewType.ViewType_View3D) return null;
            Renga.IModelView? viewModel = view as Renga.IModelView;
            if (viewModel == null) return null;
            Renga.IView3DParams? viewModelParams = viewModel as Renga.IView3DParams;
            if (viewModelParams == null) return null;

            return viewModelParams.Camera;
        }
    }

}

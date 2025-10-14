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
    }

}

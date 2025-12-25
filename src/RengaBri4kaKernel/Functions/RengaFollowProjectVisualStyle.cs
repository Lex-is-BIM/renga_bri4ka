using RengaBri4kaKernel.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Functions
{
    public class RengaFollowProjectVisualStyle
    {
        private RengaFollowProjectVisualStyle()
        {
            m_appEvents = new Renga.ApplicationEventSource(PluginData.rengaApplication);
            Start();
        }

        public static RengaFollowProjectVisualStyle CreateInstance()
        {
            if (mInstance == null) mInstance = new RengaFollowProjectVisualStyle();
            return mInstance;
        }

        public void Start()
        {
            m_appEvents.ProjectOpened += M_appEvents_ProjectOpened;
            m_appEvents.ProjectCreated += M_appEvents_ProjectCreated;
        }

        public void Stop()
        {
            m_appEvents.ProjectOpened -= M_appEvents_ProjectOpened;
            m_appEvents.ProjectCreated -= M_appEvents_ProjectCreated;
            m_appEvents.Dispose();
        }

        private void SetStyle(Renga.VisualStyle style = Renga.VisualStyle.VisualStyle_Color)
        {
            Renga.IModelView? modelView = PluginData.rengaApplication.ActiveView as Renga.IModelView;
            if (modelView == null) return;

            modelView.VisualStyle = PluginAppConfig.CreateInstance().ActiveProjectVisualStyle;
        }

        private void M_appEvents_ProjectCreated()
        {
            SetStyle();
        }

        private void M_appEvents_ProjectOpened(string obj)
        {
            SetStyle();
        }

        private Renga.ApplicationEventSource m_appEvents;
        private static RengaFollowProjectVisualStyle? mInstance = null;
    }
}

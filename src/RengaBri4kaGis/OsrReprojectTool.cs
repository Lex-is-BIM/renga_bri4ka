using System;
using System.Collections.Generic;
using System.Text;

using OSGeo.OSR;
using OSGeo.OGR;

namespace RengaBri4kaGis
{
    public class OsrReprojectTool
    {
        public OsrReprojectTool(OsrSpatialReference? cs1, OsrSpatialReference? cs2)
        {
            if ((cs1 == null || cs2 == null) || (cs1.mSpatialReference == null || cs2.mSpatialReference == null)) mCoordinateTransformation = null;
            else
            {
                bool cs1_axis = cs1.IsAxisVertical();
                bool cs2_axis = cs2.IsAxisVertical();

                if (cs1_axis && cs2_axis)
                {
                    need_swap_source = !need_swap_source;
                    need_swap_target = !need_swap_target;
                }
                //WGS84 -> UTM84-36N
                else if (cs1_axis && !cs2_axis)
                {
                    need_swap_source = !need_swap_source;
                }
                //UTM84-36N -> WGS84 
                else if (!cs1_axis && cs2_axis)
                {
                    need_swap_target = !need_swap_target;
                }

                mCoordinateTransformation = new CoordinateTransformation(cs1.mSpatialReference, cs2.mSpatialReference);
            }
        }

        public OsrReprojectTool(string cs1Wkt, string cs2Wkt)
        {
            SpatialReference sp1 = new SpatialReference(cs1Wkt);
            SpatialReference sp2 = new SpatialReference(cs2Wkt);

            if (sp1 == null | sp2 == null) mCoordinateTransformation = null;
            else mCoordinateTransformation = new CoordinateTransformation(sp1, sp2);
        }

        public void Reproject(ref OgrGeometry geometry)
        {
            if (geometry.mGeometry != null)
            {
                if (this.need_swap_source) geometry.mGeometry.SwapXY();
                if (this.mCoordinateTransformation != null) geometry.mGeometry.Transform(this.mCoordinateTransformation);

                if (this.need_swap_target) geometry.mGeometry.SwapXY();
            }
        }

        private bool need_swap_source = false;
        private bool need_swap_target = false;

        private CoordinateTransformation? mCoordinateTransformation = null;
    }
}

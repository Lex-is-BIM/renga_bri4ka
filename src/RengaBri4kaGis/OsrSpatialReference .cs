using System;
using System.Collections.Generic;
using System.Text;

using OSGeo.OSR;

namespace RengaBri4kaGis
{
    public class OsrSpatialReference
    {
        internal OsrSpatialReference(SpatialReference SpatialReference)
        {
            mSpatialReference = SpatialReference;
        }

        public OsrSpatialReference(string Wkt)
        {
            mSpatialReference = new SpatialReference(Wkt);
        }

        public string? GetWkt()
        {
            string? srs_wkt = null;
            if (mSpatialReference != null)
            {
                mSpatialReference.ExportToPrettyWkt(out srs_wkt, 1);
            }
            return srs_wkt;
        }

        public bool IsAxisVertical(int axisNum = 0)
        {
            if (mSpatialReference == null) return false;
            var orintInfo = mSpatialReference.GetAxisOrientation(null, axisNum);
            if (orintInfo == AxisOrientation.OAO_North | orintInfo == AxisOrientation.OAO_South) return true;
            else return false;
        }

        internal SpatialReference? mSpatialReference;
    }
}

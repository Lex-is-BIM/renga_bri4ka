using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Text;

namespace RengaBri4kaGis
{
    public class OgrFeature
    {
        internal OgrFeature(Feature feature, OgrFeatureDefn? fDefn)
        {
            Properties = new Dictionary<string, string>();
            mFeature = feature;
            if (fDefn != null) ReadProperties(fDefn);
        }

        private void ReadProperties(OgrFeatureDefn fDef)
        {
           
            if (mFeature == null) return;

            for (int iField = 0; iField < mFeature.GetFieldCount(); iField++)
            {
                FieldDefn fdef = fDef.mFeatureDefn.GetFieldDefn(iField);

                if (mFeature.IsFieldSet(iField))
                {
                    string propValue = "";
                    switch (fdef.GetFieldType())
                    {
                        case FieldType.OFTString:
                        case FieldType.OFTWideString:
                            propValue = mFeature.GetFieldAsString(iField);
                            break;
                        case FieldType.OFTInteger:
                            propValue = mFeature.GetFieldAsInteger(iField).ToString();
                            break;
                        case FieldType.OFTInteger64:
                            propValue = mFeature.GetFieldAsInteger64(iField).ToString();
                            break;
                        case FieldType.OFTReal:
                            propValue = mFeature.GetFieldAsDouble(iField).ToString();
                            break;
                    }

                    Properties[fdef.GetName()] = propValue;
                }
            }
        }
        public Dictionary<string, string> Properties { get; set; }

        public OgrGeometry? GetGeometry()
        {
            if (mFeature == null) return null;
            Geometry geom = mFeature.GetGeometryRef();
            return new OgrGeometry(geom);

        }

        public OgrGeometry? GetGeatureGeomerty()
        {
            if (mFeature == null) return null;
            var geom = mFeature.GetGeometryRef();
            if (geom == null) return null;
            return new OgrGeometry(geom);
        }


        private Feature mFeature;
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using OSGeo.OGR;
using OSGeo.GDAL;

namespace RengaBri4kaGis
{
    public class OgrFeatureDefn
    {
        internal OgrFeatureDefn(FeatureDefn featureDefn)
        {
            mFeatureDefn = featureDefn;
        }

        internal FeatureDefn mFeatureDefn;
    }

    public class OgrLayer
    {
        internal OgrLayer(Layer layer)
        {
            mLayer = layer;
        }

        public OsrSpatialReference? GetSpatialRef()
        {
            OSGeo.OSR.SpatialReference sr = mLayer.GetSpatialRef();
            if (sr != null) return new OsrSpatialReference(sr);
            return null;
        }

        public OgrFeatureDefn? GetFeatureDefn()
        {
            if (mLayer == null) return null;
            var fDef = mLayer.GetLayerDefn();
            if (fDef != null) return new OgrFeatureDefn(fDef);
            return null;
        }

        public List<OgrFeature>? GetFeatures()
        {
            if (mLayer == null) return null;
            var featureLists = new List<OgrFeature>();

            OgrFeatureDefn? fDefn = GetFeatureDefn();

            Feature feat;
            while ((feat = mLayer.GetNextFeature()) != null)
            {
                OgrFeature? f = new OgrFeature(feat, fDefn);
            }

            return featureLists;
        }

        private Layer mLayer;
    } 
    public class OgrDataSource
    {
        public OgrDataSource(string filePath)
        {
            this.mDataSource = Ogr.Open(filePath, 0);
        }

        public bool IsNull()
        {
            if (mDataSource == null) return true;
            return false;
        }

        public int GetLayersCount()
        {
            if (mDataSource == null) return -1;
            return mDataSource.GetLayerCount();
        }

        public OgrLayer? GetLayer (int layerIndex)
        {
            if (mDataSource == null || layerIndex < GetLayersCount()) return null;
            var layer = this.mDataSource.GetLayerByIndex(layerIndex);
            if (layer != null) return new OgrLayer(layer);
            return null;
        }

        private DataSource? mDataSource;
    }
}

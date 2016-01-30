using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Topology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesis
{
    public static class MapLayerCollectionExtensions
    {
        public static IMapLayer Insert(this IMapLayerCollection collection, int index, IDataSet dataset)
        {
            IFeatureSet featureSet = dataset as IFeatureSet;
            if (featureSet != null)
            {
                return collection.Insert(index, featureSet);
            }
            IRaster raster = dataset as IRaster;
            if (raster != null)
            {
                return collection.Insert(index, raster);
            }
            IImageData image = dataset as IImageData;
            if (image != null)
            {
                return collection.Insert(index, image);
            }
            return null;
        }

        public static IMapFeatureLayer Insert(this IMapLayerCollection collection, int index, IFeatureSet featureSet)
        {
            if (featureSet != null)
            {
                featureSet.ProgressHandler = collection.ProgressHandler;
                if ((featureSet.FeatureType == FeatureType.Point) || (featureSet.FeatureType == FeatureType.MultiPoint))
                {
                    IMapPointLayer item = new MapPointLayer(featureSet);
                    collection.Insert(index, item);
                    item.ProgressHandler = collection.ProgressHandler;
                    return item;
                }
                if (featureSet.FeatureType == FeatureType.Line)
                {
                    IMapLineLayer layer2 = new MapLineLayer(featureSet);
                    collection.Insert(index, layer2);
                    layer2.ProgressHandler = collection.ProgressHandler;
                    return layer2;
                }
                if (featureSet.FeatureType == FeatureType.Polygon)
                {
                    IMapPolygonLayer layer3 = new MapPolygonLayer(featureSet);
                    collection.Insert(index, layer3);
                    layer3.ProgressHandler = collection.ProgressHandler;
                    return layer3;
                }
            }
            return null;
        }
    }
}

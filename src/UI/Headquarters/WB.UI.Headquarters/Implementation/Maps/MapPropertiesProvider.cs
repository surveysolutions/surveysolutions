using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using Esri.ArcGISRuntime.Mapping;
using WB.Core.BoundedContexts.Headquarters.Maps;

namespace WB.UI.Headquarters.Implementation.Maps
{
    public class MapPropertiesProvider : IMapPropertiesProvider
    {
        public async Task<MapProperties> GetMapPropertiesFromFileAsync(string pathToMap, MapType mapType)
        {
            if (!Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.IsInitialized)
                Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.InstallPath = HostingEnvironment.MapPath(@"~/bin");
            switch (mapType)
            {
                case MapType.Tpk:
                {
                    TileCache titleCache = new TileCache(pathToMap);
                    await titleCache.LoadAsync();
                    ArcGISTiledLayer layer = new ArcGISTiledLayer(titleCache);

                    await layer.LoadAsync();
                    var properties = new MapProperties()
                    {
                        Wkid = titleCache.TileInfo.SpatialReference.Wkid,
                        XMax = titleCache.FullExtent.XMax,
                        XMin = titleCache.FullExtent.XMin,

                        YMax = titleCache.FullExtent.YMax,
                        YMin = titleCache.FullExtent.YMin,

                        MaxScale = layer.MaxScale,
                        MinScale = layer.MinScale
                    };

                    return properties;
                }
                case MapType.Mmpk:
                {
                    MobileMapPackage package = await MobileMapPackage.OpenAsync(pathToMap);

                    if (package.Maps.Count > 0)
                    {
                        var map =  package.Maps.First();
                        await map.LoadAsync();

                        var properties = new MapProperties()
                        {
                            Wkid = package.Item.Extent.SpatialReference.Wkid,
                            XMax = package.Item.Extent.XMax,
                            XMin = package.Item.Extent.XMin,

                            YMax = package.Item.Extent.YMax,
                            YMin = package.Item.Extent.YMin,

                            MaxScale = map.MaxScale,
                            MinScale = map.MinScale
                        };

                        return properties;
                    }
                    return null;
                }

                default:
                    throw new ArgumentException("Unsupported map type");
            }
            
        }
    }
}
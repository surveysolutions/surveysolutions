using System.Threading.Tasks;
using System.Web.Hosting;
using Esri.ArcGISRuntime.Mapping;
using WB.Core.BoundedContexts.Headquarters.Maps;

namespace WB.UI.Headquarters.Implementation.Maps
{
    public class MapPropertiesProvider : IMapPropertiesProvider
    {
        public async Task<MapProperties> GetMapPropertiesFromFileAsync(string pathToMap)
        {
            if (!Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.IsInitialized)
                Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.InstallPath = HostingEnvironment.MapPath(@"~/bin");

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
    }
}
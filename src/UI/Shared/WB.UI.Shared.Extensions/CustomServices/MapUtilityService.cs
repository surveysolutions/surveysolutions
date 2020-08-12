using System;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.MapService;

namespace WB.UI.Shared.Extensions.CustomServices
{
    public class MapUtilityService
    {
        private static async Task<Basemap> GetLocalMap(IFileSystemAccessor fileSystemAccessor, MapDescription existingMap)
        {
            try
            {
                switch (fileSystemAccessor.GetFileExtension(existingMap.MapFullPath))
                {
                    case ".mmpk":
                    {
                        MobileMapPackage package = await MobileMapPackage.OpenAsync(existingMap.MapFullPath).ConfigureAwait(false);
                        if (package.Maps.Count > 0)
                        {
                            {
                                var basemap = package.Maps.First().Basemap.Clone();
                                return basemap;
                            }
                        }
                        break;
                    }
                    case ".tpk":
                    {
                        TileCache titleCache = new TileCache(existingMap.MapFullPath);
                        var layer = new ArcGISTiledLayer(titleCache)
                        {
                            //zoom to any level
                            //if area is out of the map
                            // should be available to navigate

                            MinScale = 100000000,
                            MaxScale = 1
                        };

                        await layer.LoadAsync().ConfigureAwait(false);
                        return new Basemap(layer);

                    }
                    case ".tif":
                    {
                        Raster raster = new Raster(existingMap.MapFullPath);
                        RasterLayer newRasterLayer = new RasterLayer(raster);
                        await newRasterLayer.LoadAsync().ConfigureAwait(false);

                        //add error display
                        //
                        if (newRasterLayer.SpatialReference.IsProjected)
                        {
                            return new Basemap(newRasterLayer);
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static async Task<Basemap> GetBaseMap(IFileSystemAccessor fileSystemAccessor, MapDescription existingMap)
        {
            if (existingMap == null) return null;

            switch (existingMap.MapType)
            {
                case MapType.OnlineImagery:
                    return Basemap.CreateImagery();
                case MapType.OnlineImageryWithLabels:
                    return Basemap.CreateImageryWithLabels();
                case MapType.OnlineOpenStreetMap:
                    return Basemap.CreateOpenStreetMap();
                case MapType.LocalFile:
                    return await GetLocalMap(fileSystemAccessor, existingMap);
                default:
                    return null;
            }
        }
    }
}

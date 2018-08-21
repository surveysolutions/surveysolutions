using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Hosting;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.Infrastructure.FileSystem;

namespace WB.UI.Headquarters.Implementation.Maps
{
    public class MapService : IMapService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public MapService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            if (!Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.IsInitialized)
                Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.InstallPath = HostingEnvironment.MapPath(@"~/bin");
        }

        public async Task<MapProperties> GetMapPropertiesFromFileAsync(string pathToMap)
        {
            var fileExtension = this.fileSystemAccessor.GetFileExtension(pathToMap);

            switch (fileExtension)
            {
                case ".tpk":
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
                case ".mmpk":
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
                case ".tif":
                {
                    Raster raster = new Raster(pathToMap);
                    RasterLayer newRasterLayer = new RasterLayer(raster);
                    try
                    {
                        await newRasterLayer.LoadAsync();

                        //add error display
                        if (!newRasterLayer.SpatialReference.IsProjected)
                            throw new ArgumentException($"Geotif is not projected. {pathToMap}");

                        var properties = new MapProperties()
                        {
                            Wkid = newRasterLayer.SpatialReference.Wkid,
                            XMax = newRasterLayer.FullExtent.XMax,
                            XMin = newRasterLayer.FullExtent.XMin,

                            YMax = newRasterLayer.FullExtent.YMax,
                            YMin = newRasterLayer.FullExtent.YMin,

                            MaxScale = newRasterLayer.MaxScale,
                            MinScale = newRasterLayer.MinScale
                        };

                        return properties;
                    }
                    finally
                    {
                        //temporary soulution
                        //waiting for fix from Esri
                        FieldInfo fieldInfo =
                            raster.GetType().GetField("_coreReference", BindingFlags.NonPublic | BindingFlags.Instance);
                        (fieldInfo.GetValue(raster) as IDisposable ).Dispose();


                        FieldInfo fieldInfoNewRasterLayer =
                            newRasterLayer.GetType().GetField("_coreReference", BindingFlags.NonPublic | BindingFlags.Instance);
                        (fieldInfoNewRasterLayer.GetValue(newRasterLayer) as IDisposable).Dispose();
                    }
                }

                default:
                    throw new ArgumentException("Unsupported map type");
            }
            
        }

        public bool IsEngineEnabled()
        {
            try
            {
                if (!Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.IsInitialized)
                    Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();

                return Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.IsInitialized;
            }
            catch
            {
                return false;
            }
        }
    }
}

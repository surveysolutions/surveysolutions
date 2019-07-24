using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Hosting;
using Esri.ArcGISRuntime.Geometry;
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
                    var properties = GetProperties(titleCache.FullExtent);
                    properties.MaxScale = layer.MaxScale;
                    properties.MinScale = layer.MinScale;

                    return properties;
                }
                case ".mmpk":
                {
                    MobileMapPackage package = await MobileMapPackage.OpenAsync(pathToMap);

                    if (package.Maps.Count > 0)
                    {
                        var map =  package.Maps.First();
                        await map.LoadAsync();

                        var properties = GetProperties(package.Item.Extent);
                        properties.MaxScale = map.MaxScale;
                        properties.MinScale = map.MinScale;

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
                            throw new ArgumentException($"Geotiff is not projected. {this.fileSystemAccessor.GetFileName(pathToMap)}");
                        
                        if (newRasterLayer.FullExtent.SpatialReference.Wkid == 0)
                        {
                            SpatialReference reference = new SpatialReference(102100);
                            var projectedExtent = GeometryEngine.Project(newRasterLayer.FullExtent, reference);
                                
                            return GetProperties(projectedExtent.Extent);
                        }
                        else
                            return GetProperties(newRasterLayer.FullExtent);
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

        private MapProperties GetProperties(Envelope envelope)
        {
            return new MapProperties()
            {
                Wkid = envelope.SpatialReference.Wkid,
                //WkText = envelope.SpatialReference.WkText,

                XMax = envelope.XMax,
                XMin = envelope.XMin,

                YMax = envelope.YMax,
                YMin = envelope.YMin
            };
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

﻿using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Symbology;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.MapService;

namespace WB.UI.Shared.Extensions.Services
{
    public interface IMapUtilityService
    {
        Task<Basemap> GetBaseMap(MapDescription existingMap);
        Task<FeatureLayer> GetShapefileAsFeatureLayer(string fullPathToShapefile);
        
        Task<FeatureLayer> GetShapefileAsFeatureLayer(ShapefileFeatureTable shapefile);
    }

    public class MapUtilityService : IMapUtilityService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IEnumeratorArchiveUtils archiveUtils;
        private readonly ILogger logger;

        public MapUtilityService(IFileSystemAccessor fileSystemAccessor,
            IEnumeratorArchiveUtils archiveUtils,
            IUserInteractionService userInteractionService,
            ILogger logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.logger = logger;
            this.UserInteractionService = userInteractionService;
        }

        public IUserInteractionService UserInteractionService { get; set; }

        private async Task<Basemap> GetLocalMap(MapDescription existingMap)
        {
            try
            {
                var fileExtension = fileSystemAccessor.GetFileExtension(existingMap.MapFullPath);
                switch (fileExtension)
                {
                    case ".mmpk":
                    {
                        MobileMapPackage package = await MobileMapPackage.OpenAsync(existingMap.MapFullPath).ConfigureAwait(false);
                        if (package.Maps.Count > 0)
                        {
                            {
                                var basemap = package.Maps.First()?.Basemap?.Clone();
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
                        if (newRasterLayer.LoadError == null)
                        {
                            return new Basemap(newRasterLayer);
                        }
                        break;
                    }
                    default:
                        throw new Exception($"Unsupported map type {fileExtension}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Can't load map {existingMap.MapFullPath}", e);
            }

            return null;
        }

        public async Task<Basemap> GetBaseMap(MapDescription existingMap)
        {
            if (existingMap == null) return null;

            if (existingMap.MapType == MapType.LocalFile)
                return await GetLocalMap(existingMap);
            
            //if map is failing to load 
            //if it's loaded, it's not working to collect data
            try
            {
                switch (existingMap.MapType)
                {
                    case MapType.OnlineImagery:
                        await new Basemap(BasemapStyle.ArcGISImageryStandard).LoadAsync();
                        return new Basemap(BasemapStyle.ArcGISImageryStandard);
                    case MapType.OnlineImageryWithLabels:
                        await new Basemap(BasemapStyle.ArcGISImagery).LoadAsync();
                        return new Basemap(BasemapStyle.ArcGISImagery);
                    case MapType.OnlineOpenStreetMap:
                        await new Basemap(BasemapStyle.OSMStandard).LoadAsync();
                        return new Basemap(BasemapStyle.OSMStandard);
                }
            }
            catch (Exception e)
            {
                logger.Error("Cant load online map " + existingMap.MapType, e);
                this.UserInteractionService.ShowToast("The map is not available. Please select a different map or retry later");
            }
            
            return null;
        }

        public async Task<FeatureLayer> GetShapefileAsFeatureLayer(ShapefileFeatureTable myShapefile)
        {
            // Create a feature layer to display the shapefile
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);
            await newFeatureLayer.LoadAsync();

            // Create a StringBuilder to create the label definition JSON string
            StringBuilder addressLabelsBuilder = new StringBuilder();
            addressLabelsBuilder.AppendLine("{");
            //     Define a labeling expression that will show the address attribute value
            addressLabelsBuilder.AppendLine("\"labelExpressionInfo\": {");
            addressLabelsBuilder.AppendLine("\"expression\": \"return $feature.label;\"},");
            //     Align labels horizontally
            addressLabelsBuilder.AppendLine("\"labelPlacement\": \"esriServerPolygonPlacementAlwaysHorizontal\",");
            //     Use a green bold text symbol
            addressLabelsBuilder.AppendLine("\"symbol\": {");
            addressLabelsBuilder.AppendLine("\"haloColor\": [255,255,255,255],");
            addressLabelsBuilder.AppendLine("\"haloSize\": 2,");
            addressLabelsBuilder.AppendLine("\"horizontalAlignment\": \"center\",");
            addressLabelsBuilder.AppendLine("\"color\": [255,0,0,255],");
            addressLabelsBuilder.AppendLine("\"font\": {\"size\": 12, \"weight\": \"bold\"},");
            addressLabelsBuilder.AppendLine("\"type\": \"esriTS\"}");
            addressLabelsBuilder.AppendLine("}");

            // Get the label definition string
            var addressLabelsJson = addressLabelsBuilder.ToString();

            // Create a new LabelDefintion object using the static FromJson method
            LabelDefinition labelDef = LabelDefinition.FromJson(addressLabelsJson);

            // Clear the current collection of label definitions (if any)
            newFeatureLayer.LabelDefinitions.Clear();

            // Add this label definition to the collection
            newFeatureLayer.LabelDefinitions.Add(labelDef);

            // Make sure labeling is enabled for the layer
            newFeatureLayer.LabelsEnabled = true;

            var symbolForRenderer = CreateSymbolForRenderer(myShapefile.GeometryType, Color.Red);
            var alternateRenderer = new SimpleRenderer(symbolForRenderer);

            RendererSceneProperties myRendererSceneProperties = alternateRenderer.SceneProperties;
            myRendererSceneProperties.ExtrusionMode = ExtrusionMode.Minimum;

            newFeatureLayer.Renderer = alternateRenderer;

            return newFeatureLayer;
        }

        public async Task<FeatureLayer> GetShapefileAsFeatureLayer(string fullPathToShapefile)
        {
            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(fullPathToShapefile);
            
            return await GetShapefileAsFeatureLayer(myShapefile);
        }
        
        private Symbol CreateSymbolForRenderer(GeometryType rendererType, Color color)
        {
            switch (rendererType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, color, 6);
                case GeometryType.Polygon:
                case GeometryType.Polyline:
                default:
                    SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, color, 2.0);
                    return new SimpleFillSymbol(SimpleFillSymbolStyle.Null, Color.White, lineSymbol);
            }
        }
    }
}

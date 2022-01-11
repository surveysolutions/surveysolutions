using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Symbology;
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
                        if (newRasterLayer.LoadError == null)
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

        public static async Task<FeatureLayer> GetShapefileAsFeatureLayer(string fullPattToFSapefile)
        {
            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(fullPattToFSapefile);
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

            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2.0);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Null, Color.White, lineSymbol);
            
            var alternateRenderer = new SimpleRenderer(fillSymbol);
            
            RendererSceneProperties myRendererSceneProperties = alternateRenderer.SceneProperties;
            myRendererSceneProperties.ExtrusionMode = ExtrusionMode.Minimum;

            newFeatureLayer.Renderer = alternateRenderer;

            return newFeatureLayer;
        }
    }
}

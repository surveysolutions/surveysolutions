using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using GeometryType = WB.Core.SharedKernels.Questionnaire.Documents.GeometryType;

namespace WB.UI.Shared.Extensions.CustomServices.AreaEditor
{
    public class AreaEditorViewModel : BaseViewModel<AreaEditorViewModelArgs>
    {
        public Action<AreaEditorResult> OnAreaEditCompleted;

        readonly ILogger logger;
        private readonly IMapService mapService;
        private readonly IUserInteractionService userInteractionService;

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IMvxNavigationService navigationService;

        public AreaEditorViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMapService mapService,
            IUserInteractionService userInteractionService,
            ILogger logger,
            IFileSystemAccessor fileSystemAccessor,
            IMvxNavigationService navigationService)
            : base(principal, viewModelNavigationService)
        {
            this.userInteractionService = userInteractionService;
            this.mapService = mapService;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
            this.navigationService = navigationService;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            this.AvailableShapefiles =
                new MvxObservableCollection<ShapefileDescription>(this.mapService.GetAvailableShapefiles());

            var localMaps = this.mapService.GetAvailableMaps();
            var defaultMap = this.mapService.PrepareAndGetDefaultMap();
            localMaps.Add(defaultMap);

            this.AvailableMaps = new MvxObservableCollection<MapDescription>(localMaps);
            this.MapsList = this.AvailableMaps.Select(x => x.MapName).ToList();

            if (this.AvailableMaps.Count == 0)
                return;

            if (!string.IsNullOrEmpty(this.MapName) && this.MapsList.Contains(this.MapName))
            {
                this.SelectedMap = this.MapName;
            }
            else
            {
                this.SelectedMap = this.MapsList.FirstOrDefault();
            }

            Basemap basemap = await GetBaseMap(defaultMap).ConfigureAwait(false);
            this.Map = new Map(basemap);

            this.Map.Loaded += async delegate (object sender, EventArgs e)
            {
                await UpdateBaseMap().ConfigureAwait(false);
                await EditGeometry().ConfigureAwait(false);
            };
        }

        private MvxObservableCollection<MapDescription> availableMaps = new MvxObservableCollection<MapDescription>();
        public MvxObservableCollection<MapDescription> AvailableMaps
        {
            get => this.availableMaps;
            protected set => this.RaiseAndSetIfChanged(ref this.availableMaps, value);
        }

        private MvxObservableCollection<ShapefileDescription> availableShapefiles = new MvxObservableCollection<ShapefileDescription>();
        public MvxObservableCollection<ShapefileDescription> AvailableShapefiles
        {
            get => this.availableShapefiles;
            protected set => this.RaiseAndSetIfChanged(ref this.availableShapefiles, value);
        }

        private List<string> mapsList = new List<string>();
        public List<string> MapsList
        {
            get => this.mapsList;
            set => this.RaiseAndSetIfChanged(ref this.mapsList, value);
        }

        private string selectedMap;
        public string SelectedMap
        {
            get => this.selectedMap;
            set => this.RaiseAndSetIfChanged(ref this.selectedMap, value);
        }


        public async Task<Basemap> GetBaseMap(MapDescription existingMap)
        {
            if (existingMap != null)
            {
                var mapFileExtention = this.fileSystemAccessor.GetFileExtension(existingMap.MapFullPath);

                switch (mapFileExtention)
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
            return null;
        }

        public async Task UpdateBaseMap()
        {
            var existingMap = this.AvailableMaps.FirstOrDefault(x => x.MapName == this.SelectedMap);

            if (existingMap != null)
            {
                var basemap = await GetBaseMap(existingMap);

                this.Map.Basemap = basemap;

                if (basemap?.BaseLayers[0]?.FullExtent != null)
                    await MapView.SetViewpointGeometryAsync(basemap.BaseLayers[0].FullExtent);
            }
        }


        public override void Prepare(AreaEditorViewModelArgs parameter)
        {
            this.MapName = parameter.MapName;
            this.requestedGeometryType = parameter.RequestedGeometryType;

            if (string.IsNullOrEmpty(parameter.Geometry)) return;

            this.Geometry = Geometry.FromJson(parameter.Geometry);

            this.UpdateLabels(this.Geometry);
        }

        private Geometry Geometry { set; get; }
        public string MapName { set; get; }

        private WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? requestedGeometryType;

        private Map map;
        public Map Map
        {
            get => this.map;
            set => this.RaiseAndSetIfChanged(ref this.map, value);
        }

        public MapView MapView { get; set; }

        public IMvxAsyncCommand<MapDescription> SwitchMapCommand => new MvxAsyncCommand<MapDescription>(async (mapDescription) =>
        {
            this.SelectedMap = mapDescription.MapName;
            IsPanelVisible = false;

            var geometry = this.MapView.SketchEditor.Geometry;
            await this.UpdateBaseMap();

            //update internal structures
            //Spatialreferense of new map could differ from initial
            if (geometry != null)
            {
                if (this.MapView != null && geometry != null && !this.MapView.SpatialReference.IsEqual(geometry.SpatialReference))
                    geometry = GeometryEngine.Project(geometry, this.MapView.SpatialReference);

                this.MapView?.SketchEditor.ClearGeometry();
                this.MapView?.SketchEditor.ReplaceGeometry(geometry);
            }
        });


        public IMvxAsyncCommand RotateMapToNorth => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointRotationAsync(0));

        public IMvxAsyncCommand ZoomMapIn => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointScaleAsync(this.MapView.MapScale / 1.3));

        public IMvxAsyncCommand ZoomMapOut => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointScaleAsync(this.MapView.MapScale * 1.3));

        public IMvxCommand SaveAreaCommand => new MvxCommand(() =>
        {
            var command = this.MapView.SketchEditor.CompleteCommand;
            if (this.MapView.SketchEditor.CompleteCommand.CanExecute(command))
            {
                this.MapView.SketchEditor.CompleteCommand.Execute(command);
            }
            else
            {
                this.userInteractionService.ShowToast(UIResources.AreaMap_NoChangesInfo);
            }
        });

        public IMvxAsyncCommand CancelCommand => new MvxAsyncCommand(async () =>
        {
            var handler = this.OnAreaEditCompleted;
            handler?.Invoke(null);
            await this.navigationService.Close(this);
        });


        public IMvxAsyncCommand LoadShapefile => new MvxAsyncCommand(async () =>
        {
            if (AvailableShapefiles.Count < 1)
                return;

            try
            {
                // Open the shapefile
                ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(AvailableShapefiles.First().FullPath);
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
                addressLabelsBuilder.AppendLine("\"color\": [0,255,50,255],");
                addressLabelsBuilder.AppendLine("\"font\": {\"size\": 18, \"weight\": \"bold\"},");
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


                SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Aqua, 1.0);
                SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(20, Color.Aquamarine), lineSymbol);

                var alternateRenderer = new SimpleRenderer(fillSymbol);
                
                newFeatureLayer.Renderer = alternateRenderer;

                // Add the feature layer to the map
                this.MapView.Map.OperationalLayers.Add(newFeatureLayer);

                // Zoom the map to the extent of the shapefile
                await this.MapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);

            }
            catch (Exception e)
            {
                logger.Error("Error on shapefile loading", e);
            }
        });

        public IMvxCommand SwitchLocatorCommand => new MvxCommand(() =>
        {
            if (!IsLocationServiceSwitchEnabled)
                return;

            //try to workaround Esri crash with location service
            //Esri case 02209395
            try
            {
                IsLocationServiceSwitchEnabled = false;

                if (!this.MapView.LocationDisplay.IsEnabled)
                    this.MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;

                if (!this.MapView.LocationDisplay.IsEnabled &&
                    !this.MapView.LocationDisplay.Started &&
                    (this.MapView.LocationDisplay.DataSource != null &&
                     !this.MapView.LocationDisplay.DataSource.IsStarted))
                {
                    this.MapView.LocationDisplay.IsEnabled = true;
                    this.MapView.LocationDisplay.LocationChanged += LocationDisplayOnLocationChanged;
                }

                else
                {
                    this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;
                    this.MapView.LocationDisplay.IsEnabled = false;
                }
            }
            catch (ArgumentException exc)
            {
                logger.Error("Error occurred on map location switch.", exc);
            }
            finally
            {
                //workaround for maps location service error
                Task.Run(() =>
                {
                    Thread.Sleep(5000);
                    IsLocationServiceSwitchEnabled = true;
                });
            }

        });

        private void LocationDisplayOnLocationChanged(object sender, Location e)
        {
            //show only once
            this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;

            if (e.Position == null) { return; }

            var extent = this.MapView.Map.Basemap.BaseLayers[0].FullExtent;

            var point = GeometryEngine.Project(e.Position, extent.SpatialReference);

            if (!GeometryEngine.Contains(extent, point))
            {
                this.userInteractionService.ShowToast(UIResources.AreaMap_LocationOutOfBoundaries);
            }
        }

        public IMvxCommand SwitchPanelCommand => new MvxCommand(() =>
        {
            IsPanelVisible = !IsPanelVisible;
        });

        public IMvxAsyncCommand StartEditAreaCommand => new MvxAsyncCommand(async () => await EditGeometry());

        private async Task EditGeometry()
        {
            this.IsEditing = true;

            try
            {
                this.MapView.SketchEditor.GeometryChanged += delegate (object sender, GeometryChangedEventArgs args)
                {
                    var geometry = args.NewGeometry;
                    try
                    {
                        this.UpdateLabels(geometry);
                    }
                    catch
                    {
                    }

                    this.CanUndo =
                        this.MapView.SketchEditor.UndoCommand.CanExecute(this.MapView.SketchEditor.UndoCommand);
                    this.CanSave =
                        this.MapView.SketchEditor.CompleteCommand.CanExecute(this.MapView.SketchEditor.CompleteCommand);
                };

                if (this.Geometry == null)
                {
                    await this.MapView.SetViewpointRotationAsync(0).ConfigureAwait(false); //workaround to fix Map is not prepared Esri error.
                }
                else
                {
                    if (this.Geometry.GeometryType == Esri.ArcGISRuntime.Geometry.GeometryType.Point)
                        await this.MapView.SetViewpointCenterAsync(this.Geometry as MapPoint).ConfigureAwait(false);
                    else
                        await this.MapView.SetViewpointGeometryAsync(this.Geometry, 120).ConfigureAwait(false);
                }



                var result = await GetGeometry(this.requestedGeometryType, this.Geometry).ConfigureAwait(false);

                var position = this.MapView.LocationDisplay.Location.Position;
                double? dist = null;
                if (position != null)
                {
                    var point = GeometryEngine.Project(position, this.MapView.SpatialReference);
                    dist = GeometryEngine.Distance(result, point);
                }

                //project to geocoordinates
                SpatialReference reference = new SpatialReference(4326);
                var projectedGeometry = GeometryEngine.Project(result, reference);

                string coordinates = string.Empty;
                if (projectedGeometry != null)
                {
                    switch (projectedGeometry.GeometryType)
                    {
                        case Esri.ArcGISRuntime.Geometry.GeometryType.Polygon:
                            var polygon = projectedGeometry as Polygon;
                            var polygonCoordinates = polygon.Parts[0].Points.Select(coordinate => $"{coordinate.X.ToString(CultureInfo.InvariantCulture)},{coordinate.Y.ToString(CultureInfo.InvariantCulture)}").ToList();
                            coordinates = string.Join(";", polygonCoordinates);
                            break;
                        case Esri.ArcGISRuntime.Geometry.GeometryType.Point:
                            var point = projectedGeometry as MapPoint;
                            coordinates = $"{point.X.ToString(CultureInfo.InvariantCulture)},{point.Y.ToString(CultureInfo.InvariantCulture)}";
                            break;
                        case Esri.ArcGISRuntime.Geometry.GeometryType.Polyline:
                            var polyline = projectedGeometry as Polyline;
                            coordinates = string.Join(";", polyline.Parts[0].Points.Select(coordinate => $"{coordinate.X.ToString(CultureInfo.InvariantCulture)},{coordinate.Y.ToString(CultureInfo.InvariantCulture)}").ToList());
                            break;
                    }
                }

                var resultArea = new AreaEditorResult()
                {
                    Geometry = result?.ToJson(),
                    MapName = this.SelectedMap,
                    Coordinates = coordinates,
                    Area = GetGeometryArea(result),
                    Length = GetGeometryLength(result),
                    DistanceToEditor = dist,
                    NumberOfPoints = GetGeometryPointsCount(result)
                };

                //save
                var handler = this.OnAreaEditCompleted;
                handler?.Invoke(resultArea);
            }
            finally
            {
                this.IsEditing = false;
                this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;
                await this.navigationService.Close(this);
            }
        }

        private double GetGeometryArea(Geometry geometry)
        {
            bool doesGeometrySupportDimensionsCalculation = DoesGeometrySupportAreaCalculation(geometry);
            return doesGeometrySupportDimensionsCalculation ? GeometryEngine.AreaGeodetic(geometry) : 0;
        }

        private double GetGeometryLength(Geometry geometry)
        {
            bool doesGeometrySupportDimensionsCalculation = DoesGeometrySupportLengthCalculation(geometry);
            return doesGeometrySupportDimensionsCalculation ? GeometryEngine.LengthGeodetic(geometry) : 0;
        }
        private void UpdateLabels(Geometry geometry)
        {
            var area = this.GetGeometryArea(geometry);
            var length = this.GetGeometryLength(geometry);

            this.GeometryArea = area > 0 ? string.Format(UIResources.AreaMap_AreaFormat, area.ToString("#.##")) : string.Empty;
            this.GeometryLengthLabel = length > 0 ? string.Format(
                this.requestedGeometryType == GeometryType.Polygon
                    ? UIResources.AreaMap_PerimeterFormat
                    : UIResources.AreaMap_LengthFormat, length.ToString("#.##")) : string.Empty;
        }

        private int GetGeometryPointsCount(Geometry geometry)
        {
            switch (geometry.GeometryType)
            {
                case Esri.ArcGISRuntime.Geometry.GeometryType.Point:
                    return 1;

                case Esri.ArcGISRuntime.Geometry.GeometryType.Polyline:
                    return (geometry as Polyline).Parts[0].PointCount;

                case Esri.ArcGISRuntime.Geometry.GeometryType.Polygon:
                    return (geometry as Polygon).Parts[0].PointCount;

                case Esri.ArcGISRuntime.Geometry.GeometryType.Multipoint:
                    return (geometry as Multipoint).Points.Count();
                default:
                    return 0;
            }
        }

        private bool DoesGeometrySupportAreaCalculation(Geometry geometry)
        {
            if (geometry == null)
                return false;

            if (geometry.GeometryType != Esri.ArcGISRuntime.Geometry.GeometryType.Polygon || geometry.Dimension != GeometryDimension.Area)
                return false;

            var polygon = geometry as Polygon;
            if (polygon == null)
                return false;

            if (polygon.Parts.Count < 1)
                return false;

            var readOnlyPart = polygon.Parts[0];
            if (readOnlyPart.PointCount < 3)
                return false;

            var groupedPoints = from point in readOnlyPart.Points
                                group point by new { X = point.X, Y = point.Y } into xyPoint
                                select new { X = xyPoint.Key.X, Y = xyPoint.Key.Y, Count = xyPoint.Count() };

            if (groupedPoints.Count() < 3)
                return false;

            return true;
        }

        private bool DoesGeometrySupportLengthCalculation(Geometry geometry)
        {
            if (geometry == null)
                return false;

            if (requestedGeometryType == GeometryType.Multipoint || requestedGeometryType == GeometryType.Point)
                return false;

            return true;
        }

        private string geometryArea;
        public string GeometryArea
        {
            get => this.geometryArea;
            set => this.RaiseAndSetIfChanged(ref this.geometryArea, value);
        }

        private string geometryLengthLabel;
        public string GeometryLengthLabel
        {
            get => this.geometryLengthLabel;
            set => this.RaiseAndSetIfChanged(ref this.geometryLengthLabel, value);
        }

        private void BtnUndo()
        {
            var command = this.MapView?.SketchEditor.UndoCommand;
            if (this.MapView?.SketchEditor?.UndoCommand.CanExecute(command) ?? false)
                this.MapView.SketchEditor.UndoCommand.Execute(command);
        }

        private void BtnCancelCommand()
        {
            var command = this.MapView?.SketchEditor.UndoCommand;
            while (this.MapView?.SketchEditor?.UndoCommand.CanExecute(command) ?? false)
            {
                this.MapView.SketchEditor.UndoCommand.Execute(command);
            }
        }

        public IMvxCommand UndoCommand => new MvxCommand(this.BtnUndo);
        public IMvxCommand CancelEditCommand => new MvxCommand(this.BtnCancelCommand);

        private bool isEditing;
        public bool IsEditing
        {
            get => this.isEditing;
            set => this.RaiseAndSetIfChanged(ref this.isEditing, value);
        }

        private bool isGeometryAreaVisible;
        public bool IsGeometryAreaVisible
        {
            get => this.isEditing;
            set => this.RaiseAndSetIfChanged(ref this.isGeometryAreaVisible, value);
        }

        private bool isGeometryLengthVisible;
        public bool IsGeometryLengthVisible
        {
            get => this.isEditing;
            set => this.RaiseAndSetIfChanged(ref this.isGeometryLengthVisible, value);
        }

        private bool canUndo;
        public bool CanUndo
        {
            get => this.canUndo;
            set => this.RaiseAndSetIfChanged(ref this.canUndo, value);
        }

        private bool canSave;
        public bool CanSave
        {
            get => this.canSave;
            set => this.RaiseAndSetIfChanged(ref this.canSave, value);
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.RaiseAndSetIfChanged(ref this.isInProgress, value);
        }

        private bool isPanelVisible;
        public bool IsPanelVisible
        {
            get => this.isPanelVisible;
            set => this.RaiseAndSetIfChanged(ref this.isPanelVisible, value);
        }

        private bool isLocationServiceSwitchEnabled = true;
        public bool IsLocationServiceSwitchEnabled
        {
            get => this.isLocationServiceSwitchEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isLocationServiceSwitchEnabled, value);
        }

        private async Task<Geometry> GetGeometry(GeometryType? geometryType, Geometry geometry)
        {
            switch (geometryType)
            {
                case GeometryType.Polyline:
                    {
                        IsGeometryLengthVisible = true;
                        IsGeometryAreaVisible = false;

                        return geometry == null
                            ? await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Polyline).ConfigureAwait(false)
                            : await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Polyline)
                                .ConfigureAwait(false);
                    }
                case GeometryType.Point:
                    {
                        IsGeometryLengthVisible = false;
                        IsGeometryAreaVisible = false;

                        return geometry == null
                            ? await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Point).ConfigureAwait(false)
                            : await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Point)
                                .ConfigureAwait(false);
                    }
                case GeometryType.Multipoint:
                    {
                        IsGeometryLengthVisible = false;
                        IsGeometryAreaVisible = false;

                        this.MapView.SketchEditor.Style.MidVertexSymbol = null;
                        this.MapView.SketchEditor.Style.LineSymbol = null;

                        return geometry == null ?
                            await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Polyline).ConfigureAwait(false) :
                            await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Polyline).ConfigureAwait(false);
                    }
                default:
                    {
                        IsGeometryLengthVisible = true;
                        IsGeometryAreaVisible = true;

                        return geometry == null
                            ? await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Polygon).ConfigureAwait(false)
                            : await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Polygon)
                                .ConfigureAwait(false);
                    }
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Views;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Core.ViewModels;
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
    public class AreaEditorViewModelArgs
    {
        public string Geometry { get; set; }
        public string MapName { get; set; }
        public WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? RequestedGeometryType { set; get; }
    }

    public class AreaEditorViewModel : BaseViewModel<AreaEditorViewModelArgs>
    {
        public event Action<AreaEditorResult> OnAreaEditCompleted;

        readonly ILogger logger;
        private readonly IMapService mapService;
        private readonly IUserInteractionService userInteractionService;

        private readonly IFileSystemAccessor fileSystemAccessor;

        public AreaEditorViewModel(IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMapService mapService,
            IUserInteractionService userInteractionService,
            ILogger logger,
            IFileSystemAccessor fileSystemAccessor)
            : base(principal, viewModelNavigationService)
        {
            this.userInteractionService = userInteractionService;
            this.mapService = mapService;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
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

            if (!string.IsNullOrEmpty(parameter.Geometry))
            {
                this.Geometry = Geometry.FromJson(parameter.Geometry);

                this.GeometryArea = GeometryEngine.AreaGeodetic(this.Geometry).ToString("#.##");
                this.GeometryLength = GeometryEngine.LengthGeodetic(this.Geometry).ToString("#.##");
            }
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

        private MapView mapView;
        public MapView MapView
        {
            get => this.mapView;
            set
            {
                this.mapView = value;
                mapView.ViewAttachedToWindow += delegate (object sender, View.ViewAttachedToWindowEventArgs args)
                {
                    //StartEditAreaCommand.Execute();
                };

            }
        }

        public IMvxAsyncCommand<MapDescription> SwitchMapCommand => new MvxAsyncCommand<MapDescription>(async (mapDescription) =>
        {
            this.SelectedMap = mapDescription.MapName;
            IsPanelVisible = false;

            var geometry = this.MapView.SketchEditor.Geometry;
            await this.UpdateBaseMap();

            //update internal structures
            //spatialreferense of new map could differ from initial
            if (geometry != null)
            {
                if (this.MapView != null && geometry != null && !this.MapView.SpatialReference.IsEqual(geometry.SpatialReference))
                    geometry = GeometryEngine.Project(geometry, this.MapView.SpatialReference);

                this.MapView?.SketchEditor.ClearGeometry();

                var geometryReplaced = this.MapView?.SketchEditor.ReplaceGeometry(geometry);
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

        public IMvxCommand CancelCommand => new MvxCommand(() =>
        {
            var handler = this.OnAreaEditCompleted;
            handler?.Invoke(null);
            Close(this);
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

                SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Aqua, 1.0);
                SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(30, Color.Aquamarine), lineSymbol);

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

            //temporary catch for KP-9486
            //esri was notified
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

            var point = GeometryEngine.Project(e.Position, this.MapView.SpatialReference);

            if (!GeometryEngine.Contains(this.MapView.Map.Basemap.BaseLayers[0].FullExtent, point))
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
            //if(this.Map == null)
            this.IsEditing = true;
            try
            {
                this.MapView.SketchEditor.GeometryChanged += delegate (object sender, GeometryChangedEventArgs args)
                {
                    var geometry = args.NewGeometry;
                    try
                    {
                        this.GeometryArea = GetGeometryArea(geometry).ToString("#.##");
                        this.GeometryLength = GetGeometryLenght(geometry).ToString("#.##");
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
                    await this.MapView.SetViewpointRotationAsync(0).ConfigureAwait(false); //workaround to fix Map is not prepared.
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
                            var polygonCoordinates = polygon.Parts[0].Points.Select(x => $"{x.X.ToString(CultureInfo.InvariantCulture)},{x.Y.ToString(CultureInfo.InvariantCulture)}").ToList();
                            coordinates = string.Join(";", polygonCoordinates);
                            break;
                        case Esri.ArcGISRuntime.Geometry.GeometryType.Point:
                            var point = projectedGeometry as MapPoint;
                            coordinates = $"{point.X.ToString(CultureInfo.InvariantCulture)},{point.X.ToString(CultureInfo.InvariantCulture)}";
                            break;
                        case Esri.ArcGISRuntime.Geometry.GeometryType.Polyline:
                            var polyline = projectedGeometry as Polyline;
                            coordinates = string.Join(";", polyline.Parts[0].Points.Select(x => $"{x.X.ToString(CultureInfo.InvariantCulture)},{x.Y.ToString(CultureInfo.InvariantCulture)}").ToList());
                            break;
                    }
                }

                var resultArea = new AreaEditorResult()
                {
                    Geometry = result?.ToJson(),
                    MapName = this.SelectedMap,
                    Coordinates = coordinates,
                    Area = GetGeometryArea(result),
                    Length = GetGeometryLenght(result),
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
                Close(this);
            }
        }

        private double GetGeometryArea(Geometry geometry)
        {
            bool doesGeometrySupportDimensionsCalculation = DoesGeometrySupportAreaCalculation(geometry);
            return doesGeometrySupportDimensionsCalculation ? GeometryEngine.AreaGeodetic(geometry) : 0;
        }

        private double GetGeometryLenght(Geometry geometry)
        {
            bool doesGeometrySupportDimensionsCalculation = DoesGeometrySupportLengthCalculation(geometry);
            return doesGeometrySupportDimensionsCalculation ? GeometryEngine.LengthGeodetic(geometry) : 0;
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

        private string geometryArea = "0";
        public string GeometryArea
        {
            get => this.geometryArea;
            set => this.RaiseAndSetIfChanged(ref this.geometryArea, value);
        }

        private string geometryLength = "0";
        public string GeometryLength
        {
            get => this.geometryLength;
            set => this.RaiseAndSetIfChanged(ref this.geometryLength, value);
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

        private async Task<Geometry> GetGeometry(WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? geometryType, Geometry geometry)
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

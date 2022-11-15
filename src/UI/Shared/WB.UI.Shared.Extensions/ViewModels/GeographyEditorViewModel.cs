using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Gms.Maps;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.Services;
using GeometryType = WB.Core.SharedKernels.Questionnaire.Documents.GeometryType;
using EsriGeometryType = Esri.ArcGISRuntime.Geometry.GeometryType;

namespace WB.UI.Shared.Extensions.ViewModels
{
    public class GeographyEditorViewModel : BaseMapInteractionViewModel<GeographyEditorViewModelArgs>
    {
        protected const string NeighborsLayerName = "neighbors";
        public Action<AreaEditorResult> OnAreaEditCompleted;
        
        private readonly LocationDataSource locationDataSource = new SystemLocationDataSource();
        private GraphicsOverlay locationOverlay;
        private GraphicsOverlay locationLineOverlay;
        private GeometryByTypeBuilder geometryBuilder;

        public GeographyEditorViewModel(IPrincipal principal, IViewModelNavigationService viewModelNavigationService,
            IMapService mapService, IUserInteractionService userInteractionService, ILogger logger,
            IEnumeratorSettings enumeratorSettings, IMapUtilityService mapUtilityService,
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher,
            IVirbationService vibrationService)
            : base(principal, viewModelNavigationService, mapService, userInteractionService, logger,
                enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher)
        {
            VibrationService = vibrationService;
        }

        private int? RequestedAccuracy { get; set; }
        private int? RequestedFrequency { get; set; }
        private GeometryInputMode RequestedGeometryInputMode { get; set; }
        private GeometryNeighbor[] GeographyNeighbors { get; set; }
        private Geometry Geometry { set; get; }
        private GeometryType RequestedGeometryType { get; set; }

        private IVirbationService VibrationService { get; set; }

        public string MapName { set; get; }
        public string Title { set; get; }

        public bool IsManual => RequestedGeometryInputMode == GeometryInputMode.Manual;

        public override void Prepare(GeographyEditorViewModelArgs parameter)
        {
            this.MapName = parameter.MapName;
            this.Title = parameter.Title;
            this.RequestedGeometryType = parameter.RequestedGeometryType ?? GeometryType.Polygon;
            this.RequestedAccuracy = parameter.RequestedAccuracy;
            this.RequestedFrequency = parameter.RequestedFrequency;
            this.RequestedGeometryInputMode = parameter.RequestedGeometryInputMode ?? GeometryInputMode.Manual;
            this.GeographyNeighbors = parameter.GeographyNeighbors
                .Select(n => new GeometryNeighbor()
                {
                    Id = n.Id,
                    Title = n.Title,
                    Geometry = Geometry.FromJson(n.Geometry),
                }).ToArray();

            if (string.IsNullOrEmpty(parameter.Geometry)) return;

            IsLocationServiceSwitchEnabled = IsManual;
            var geometry = Geometry.FromJson(parameter.Geometry);
            this.Geometry = GetAndFixIfNeedGeometry(geometry, RequestedGeometryType);
            this.UpdateLabels(this.Geometry);
        }

        private Geometry GetAndFixIfNeedGeometry(Geometry geometry, GeometryType requestedGeometryType)
        {
            if (requestedGeometryType == GeometryType.Multipoint &&
                geometry.GeometryType == EsriGeometryType.Polyline &&
                geometry is Polyline polyline)
            {
                var builder = new GeometryByTypeBuilder(polyline.SpatialReference, GeometryType.Multipoint);
                foreach (var mapPoint in polyline.Parts[0].Points)
                {
                    builder.AddPoint(mapPoint);
                }

                return builder.ToGeometry();
            }

            return geometry;
        }

        public IMvxAsyncCommand CancelCommand => new MvxAsyncCommand(async () =>
        {
            var handler = this.OnAreaEditCompleted;
            handler?.Invoke(null);
            await this.NavigationService.Close(this);
        });
        
        private async Task StartEditingGeometry()
        {
            this.IsEditing = true;

            try
            {
                this.MapView.SketchEditor.GeometryChanged += async delegate (object sender, GeometryChangedEventArgs args)
                {
                    var geometry = args.NewGeometry;
                    try
                    {
                        this.UpdateLabels(geometry);
                        await UpdateDrawNeighborsAsync(geometry, this.GeographyNeighbors).ConfigureAwait(false);
                    }
                    catch
                    {
                    }

                    this.CanUndo = this.MapView.SketchEditor.UndoCommand.CanExecute(this.MapView.SketchEditor.UndoCommand);
                    this.CanSave = this.MapView.SketchEditor.CompleteCommand.CanExecute(this.MapView.SketchEditor.CompleteCommand);
                };

                if (this.Geometry == null)
                {
                    await this.MapView.SetViewpointRotationAsync(0).ConfigureAwait(false); //workaround to fix Map is not prepared Esri error.
                }
                else
                {
                    if (this.MapView != null && this.MapView.Map?.SpatialReference != null && !this.MapView.Map.SpatialReference.IsEqual(Geometry.SpatialReference))
                        Geometry = GeometryEngine.Project(Geometry, this.MapView.Map.SpatialReference);

                    await SetViewpointToGeometry(this.Geometry);
                }

                await DrawNeighborsAsync(this.Geometry).ConfigureAwait(false);

                var result = await GetGeometry().ConfigureAwait(false);

                var position = this.MapView?.LocationDisplay?.Location?.Position;
                double? distanceToEditor = null;
                if (position != null)
                {
                    var point = GeometryEngine.Project(position, this.MapView.Map.SpatialReference);
                    distanceToEditor = GeometryEngine.Distance(result, point);
                }

                SaveGeometry(result, distanceToEditor);
            }
            finally
            {
                await FinishEditing();
            }
        }


        private async Task FinishEditing()
        {
            this.IsEditing = false;
            if(this.MapView?.LocationDisplay != null)
                this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;
                
            await this.NavigationService.Close(this);
        }

        private void SaveGeometry(Geometry result, double? distanceToEditor)
        {
            var geometryWgs84 = result != null
                ? GeometryEngine.Project(result, SpatialReferences.Wgs84)
                : null;
            var resultArea = new AreaEditorResult()
            {
                Geometry = geometryWgs84?.ToJson(),
                MapName = this.SelectedMap,
                Coordinates = GeometryHelper.GetProjectedCoordinates(geometryWgs84),
                Area = GeometryHelper.GetGeometryArea(geometryWgs84),
                Length = GeometryHelper.GetGeometryLength(geometryWgs84),
                DistanceToEditor = distanceToEditor,
                NumberOfPoints = GeometryHelper.GetGeometryPointsCount(geometryWgs84),
                RequestedAccuracy = RequestedAccuracy,
                RequestedFrequency = RequestedFrequency
            };

            //save
            var handler = this.OnAreaEditCompleted;
            handler?.Invoke(resultArea);
        }


        public IMvxAsyncCommand SaveAreaCommand => new MvxAsyncCommand(async() =>
        {
            if (IsManual)
            {
                if (this.MapView.SketchEditor != null)
                {
                    var command = this.MapView.SketchEditor.CompleteCommand;
                    if (this.MapView.SketchEditor.CompleteCommand.CanExecute(command))
                    {
                        this.MapView.SketchEditor.CompleteCommand.Execute(command);
                    }
                    else
                    {
                        this.UserInteractionService.ShowToast(UIResources.AreaMap_NoChangesInfo);
                    }
                }
            }
            else
            {
                collectionCancellationTokenSource?.Cancel();

                var resultGeometry = geometryBuilder.ToGeometry();
                SaveGeometry(resultGeometry, null);
                
                await FinishEditing();
            }
        });

        private void UpdateLabels(Geometry geometry)
        {
            if (geometry == null) return;
            
            var area = GeometryHelper.GetGeometryArea(geometry);
            var length = GeometryHelper.GetGeometryLength(geometry);

            this.GeometryArea = area > 0 ? string.Format(UIResources.AreaMap_AreaFormat, area.ToString("#.##")) : string.Empty;
            this.GeometryLengthLabel = length > 0 ? string.Format(
                this.RequestedGeometryType == GeometryType.Polygon
                    ? UIResources.AreaMap_PerimeterFormat
                    : UIResources.AreaMap_LengthFormat, length.ToString("#.##")) : string.Empty;
        }

        private async Task DrawNeighborsAsync(Geometry geometry)
        {
            var neighbors = this.GeographyNeighbors;
            if (neighbors == null || neighbors.Length == 0)
                return;
            
            if(this.MapView?.Map?.SpatialReference == null)
                return;
            
            var gType = this.RequestedGeometryType;
            var esriGeometryType = neighbors[0].Geometry.GeometryType;

            IEnumerable<Field> fields = new Field[]
            {
                new Field(FieldType.Text, "name", "Name", 50)
            };
            var mapViewSpatialReference = this.MapView.Map.SpatialReference;
            var neighborsFeatureCollectionTable = new FeatureCollectionTable(fields, esriGeometryType, mapViewSpatialReference)
                {
                    Renderer = CreateRenderer(gType, Color.Blue)
                };
            var overlappingNeighborsFeatureCollectionTable = new FeatureCollectionTable(fields, esriGeometryType, mapViewSpatialReference)
                {
                    Renderer = CreateRenderer(gType, Color.Orange)
                };

            List<string> overlappingTitles = new List<string>();

            for (int i = 0; i < neighbors.Length; i++)
            {
                var neighbor = neighbors[i];
                
                if (neighbor.Geometry.SpatialReference != mapViewSpatialReference)
                    neighbor.Geometry = GeometryEngine.Project(neighbor.Geometry, mapViewSpatialReference);
                
                var featureCollectionTable = neighborsFeatureCollectionTable;
                
                if (geometry != null)
                {
                    var isOverlapping = !GeometryEngine.Disjoint(geometry, neighbor.Geometry);
                    neighbor.IsOverlapping = isOverlapping;
                    
                    if (isOverlapping)
                        overlappingTitles.Add(neighbor.Title);
                    
                    featureCollectionTable = isOverlapping
                        ? overlappingNeighborsFeatureCollectionTable
                        : neighborsFeatureCollectionTable;
                }

                var feature = featureCollectionTable.CreateFeature();
                feature.Geometry = neighbor.Geometry;
                neighbor.Feature = feature;
                await featureCollectionTable.AddFeatureAsync(feature);
            }

            FeatureCollection featuresCollection = new FeatureCollection();
            featuresCollection.Tables.Add(neighborsFeatureCollectionTable);
            featuresCollection.Tables.Add(overlappingNeighborsFeatureCollectionTable);
            FeatureCollectionLayer featureCollectionLayer = new FeatureCollectionLayer(featuresCollection)
            {
                Name = NeighborsLayerName
            };

            var existedLayer = this.MapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == featureCollectionLayer.Name);
            if (existedLayer != null)
                this.MapView.Map.OperationalLayers.Remove(existedLayer);
            
            this.MapView.Map.OperationalLayers.Add(featureCollectionLayer);

            UpdateWarningMessage(overlappingTitles);
        }

        private void UpdateWarningMessage(List<string> overlappingTitles)
        {
            if (overlappingTitles.Count > 0)
            {
                var maxTitlesDisplay = 2;
                var message = UIResources.AreaMap_OverlapsWith + " " 
                    + string.Join(", ", overlappingTitles.Take(maxTitlesDisplay).ToArray());
                if (overlappingTitles.Count > maxTitlesDisplay)
                    message += " " + string.Format(UIResources.AreaMap_OverlapsWithOther,
                        overlappingTitles.Count - maxTitlesDisplay);
                this.Warning = message;
            }

            IsWarningVisible = overlappingTitles.Count > 0;
        }

        private async Task UpdateDrawNeighborsAsync(Geometry geometry, GeometryNeighbor[] neighbors)
        {
            if (neighbors == null || neighbors.Length == 0)
                return;

            var existedLayer = this.MapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == NeighborsLayerName);
            if (existedLayer == null)
                return;
            
            var featureCollectionLayer = (FeatureCollectionLayer)existedLayer;
            var featureCollectionTables = featureCollectionLayer.FeatureCollection.Tables;
            var neighborsFeatureCollectionTable = featureCollectionTables[0];
            var overlappingNeighborsFeatureCollectionTable = featureCollectionTables[1];

            List<string> overlappingTitles = new List<string>();

            foreach (var neighbor in neighbors)
            {
                var isOverlapping = geometry != null && !GeometryEngine.Disjoint(geometry, neighbor.Geometry);
                if (isOverlapping)
                    overlappingTitles.Add(neighbor.Title);
                
                if (neighbor.IsOverlapping != isOverlapping)
                {
                    var featureCollectionTable = isOverlapping
                        ? overlappingNeighborsFeatureCollectionTable
                        : neighborsFeatureCollectionTable;
                    var feature = featureCollectionTable.CreateFeature();
                    feature.Geometry = neighbor.Geometry;
                    await featureCollectionTable.AddFeatureAsync(feature);
                    
                    var oldFeatureCollectionTable = !isOverlapping
                        ? overlappingNeighborsFeatureCollectionTable
                        : neighborsFeatureCollectionTable;
                    await oldFeatureCollectionTable.DeleteFeatureAsync(neighbor.Feature);
                    
                    neighbor.Feature = feature;
                    neighbor.IsOverlapping = isOverlapping;
                }
            }

            UpdateWarningMessage(overlappingTitles);
        }

        private async Task<Geometry> GetGeometry()
        {
            Geometry geometry = Geometry;
            switch (RequestedGeometryType)
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
                            await this.MapView.SketchEditor.StartAsync(SketchCreationMode.Multipoint).ConfigureAwait(false) :
                            await this.MapView.SketchEditor.StartAsync(geometry, SketchCreationMode.Multipoint).ConfigureAwait(false);
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
        
        private Renderer CreateRenderer(GeometryType rendererType, Color color)
        {
            Symbol sym = rendererType switch
            {
                GeometryType.Point => new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, color, 18),
                GeometryType.Multipoint => new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, color, 18),
                GeometryType.Polyline => new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, color, 2),
                GeometryType.Polygon => new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, color, 2),
                _ => null
            };

            return new SimpleRenderer(sym);
        }

        public override async Task OnMapLoaded()
        {
            if (IsManual)
                await StartEditingGeometry();
            else
                await StartAuto();
        }

        private async Task StartAuto()
        {
            StartButtonText = "Initiating...";
            AddPointVisible = false;

            if(this.MapView?.Map?.SpatialReference == null)
                return;
            
            if (RequestedGeometryType is GeometryType.Polygon or GeometryType.Polyline)
            {
                //init map overlay
                locationLineOverlay = new GraphicsOverlay();
                SimpleLineSymbol locationLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3);

                if (RequestedGeometryType is GeometryType.Polygon)
                {
                    SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(22,0,0, 0), locationLineSymbol);
                    locationLineOverlay.Renderer = new SimpleRenderer(fillSymbol);
                }
                else
                    locationLineOverlay.Renderer = new SimpleRenderer(locationLineSymbol);
                
                this.MapView.GraphicsOverlays.Add(locationLineOverlay);
            }

            locationOverlay = new GraphicsOverlay();
            SimpleMarkerSymbol locationPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, System.Drawing.Color.Blue, 5);
            locationOverlay.Renderer = new SimpleRenderer(locationPointSymbol);
            this.MapView.GraphicsOverlays.Add(locationOverlay);

            //project and use current map
            geometryBuilder = new GeometryByTypeBuilder(this.MapView.Map.SpatialReference, RequestedGeometryType);

            if (this.Geometry == null)
            {
                StartButtonVisible = true;
                try
                {
                    await locationDataSource.StartAsync();
                    if (locationDataSource.Status == LocationDataSourceStatus.Started)
                    {
                        this.MapView.LocationDisplay.DataSource = locationDataSource;
                        this.MapView.LocationDisplay.IsEnabled = true;
                        this.MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                        this.MapView.LocationDisplay.Opacity = 0.5;

                        CanStartStopCollecting = true;
                        StartButtonText = "Start";
                    }
                    else
                    {
                        this.UserInteractionService.ShowToast("Error on location services start");
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Error on location start", e);
                    this.UserInteractionService.ShowToast("Error on location services start");
                }
            }
            else
            {
                if (this.MapView != null && this.MapView.Map?.SpatialReference != null 
                                         && !this.MapView.Map.SpatialReference.IsEqual(Geometry.SpatialReference))
                    Geometry = GeometryEngine.Project(Geometry, this.MapView.Map.SpatialReference);
                
                await SetViewpointToGeometry(this.Geometry);
                
                switch (this.Geometry.GeometryType)
                {
                    case EsriGeometryType.Point:
                        var newPoint = this.Geometry as MapPoint; 
                        geometryBuilder.AddPoint(newPoint);
                        locationOverlay.Graphics.Add(new Graphic(newPoint));
                        break;
                    case EsriGeometryType.Polyline:
                    {
                        var polyline = this.Geometry as Polyline;
                        foreach (var point in polyline.Parts[0].Points)
                        {
                            geometryBuilder.AddPoint(point);
                            locationOverlay.Graphics.Add(new Graphic(point));
                        }
                    }
                        break;
                    case EsriGeometryType.Polygon:
                    {
                        var polygon = this.Geometry as Polygon;
                        foreach (var point in polygon.Parts[0].Points)
                        {
                            geometryBuilder.AddPoint(point);
                            locationOverlay.Graphics.Add(new Graphic(point));
                        }
                    }
                        break;
                    case EsriGeometryType.Multipoint:
                    {
                        var multipoint = this.Geometry as Multipoint;
                        foreach (var point in multipoint.Points)
                        {
                            geometryBuilder.AddPoint(point);
                            locationOverlay.Graphics.Add(new Graphic(point));
                        }
                    }
                        break;
                    case EsriGeometryType.Unknown:
                    case EsriGeometryType.Envelope:
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (RequestedGeometryType is GeometryType.Polygon or GeometryType.Polyline)
                {
                    locationLineOverlay.Graphics.Clear();
                    // Add the updated line.
                    var geometry = geometryBuilder.ToGeometry();
                    locationLineOverlay.Graphics.Add(new Graphic(geometry));
                }
            }
            
            await DrawNeighborsAsync(this.Geometry).ConfigureAwait(false);
        }

        public override MapDescription GetSelectedMap(MvxObservableCollection<MapDescription> mapsToSelectFrom)
        {
            MapDescription mapToLoad = null;
            
            if (!string.IsNullOrEmpty(this.MapName))
            {
                mapToLoad = mapsToSelectFrom.FirstOrDefault(x => x.MapName == this.MapName);
            }

            if (!string.IsNullOrEmpty(this.LastMap))
            {
                mapToLoad = mapsToSelectFrom.FirstOrDefault(x => x.MapName == LastMap);
            }
            
            mapToLoad ??= mapsToSelectFrom.FirstOrDefault(x => x.MapType == MapType.LocalFile);
                
            return mapToLoad ?? mapsToSelectFrom.First();
        }
        
        private void BtnUndo()
        {
            if (IsManual)
            {
                var command = this.MapView?.SketchEditor.UndoCommand;
                if (this.MapView?.SketchEditor?.UndoCommand.CanExecute(command) ?? false)
                    this.MapView.SketchEditor.UndoCommand.Execute(command);
            }
            else if (RequestedGeometryInputMode == GeometryInputMode.Semiautomatic)
            {
                if(locationOverlay.Graphics.Count > 0)
                    locationOverlay.Graphics.RemoveAt(locationOverlay.Graphics.Count - 1);
                
                geometryBuilder.RemoveLastPoint();
                
                if (RequestedGeometryType is GeometryType.Polygon or GeometryType.Polyline)
                {
                    locationLineOverlay.Graphics.Clear();
                    // Add the updated line.
                    var geometry = geometryBuilder.ToGeometry();
                    locationLineOverlay.Graphics.Add(new Graphic(geometry));
                }
                
                if (RequestedGeometryType == GeometryType.Polygon)
                {
                    HasWarning = !geometryBuilder.IsCorrectlyMeasured(RequestedAccuracy);
                }
            }
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

        private bool isCollecting = false;
        public IMvxAsyncCommand StartStopCollectingCommand => new MvxAsyncCommand(this.StartCollecting);
        private Task StartCollecting()
        {
            isCollecting = !isCollecting;
            VibrationService.Enable();
            StartButtonVisible = false;

            if (RequestedGeometryInputMode == GeometryInputMode.Semiautomatic)
                AddPointVisible = true;

            if (isCollecting)
            {
                locationDataSource.LocationChanged += LocationDataSourceOnLocationChanged;
                
                if (RequestedGeometryInputMode == GeometryInputMode.Automatic)
                {
                    collectionCancellationTokenSource = new CancellationTokenSource();
                    TimeSpan period = TimeSpan.FromSeconds(RequestedFrequency ?? 10);
                    Task.Run(() => PeriodicCollectionAsync(period, collectionCancellationTokenSource.Token), collectionCancellationTokenSource.Token)
                        .ConfigureAwait(false);
                }
            }

            return Task.CompletedTask;
        }

        CancellationTokenSource collectionCancellationTokenSource;
        public async Task PeriodicCollectionAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                await AddPointToCollection();
                await Task.Delay(interval, cancellationToken);
            }
        }

        private MapPoint LastPosition { set; get; } = null;

        private void LocationDataSourceOnLocationChanged(object sender, Location e)
        {
            //filter values that not fulfill accuracy condition
            //save the latest value to temp storage

            // toast on collecting
            /*var source = e.AdditionalSourceProperties.ContainsKey("positionSource")
                ? e.AdditionalSourceProperties["positionSource"]
                : "unknown";

            this.UserInteractionService.ShowToast(
                $"Position: {e.Position}; e.HorizontalAccuracy: {e.HorizontalAccuracy}; Source: {source}; Valid: {e.HorizontalAccuracy <= RequestedAccuracy}");
                */

            if (e.HorizontalAccuracy > RequestedAccuracy) return;
            
            LastPosition = e.Position;
            CanAddPoint = true;
        }

        public IMvxAsyncCommand AddPointCommand => new MvxAsyncCommand(async() => await this.AddPoint());
        private async Task AddPoint()
        {
            // manually add point from last position
            await AddPointToCollection();
        }

        private async Task AddPointToCollection()
        {
            try
            {
                if (LastPosition != null)
                {
                    var lastPositionProjected = GeometryEngine.Project(LastPosition, geometryBuilder.SpatialReference) as MapPoint;

                    //remove Z component
                    var newPoint = new MapPoint(lastPositionProjected.X, lastPositionProjected.Y,
                        lastPositionProjected.SpatialReference);
                    
                    //reset temp collected point
                    LastPosition = null;

                    geometryBuilder.AddPoint(newPoint);
                    CanAddPoint = false;
                    locationOverlay.Graphics.Add(new Graphic(newPoint));
                    
                    if (RequestedGeometryType == GeometryType.Polygon || RequestedGeometryType == GeometryType.Polyline)
                    {
                        // Remove the old line.
                        locationLineOverlay.Graphics.Clear();
                        // Add the updated line.
                        var geometry = geometryBuilder.ToGeometry();
                        locationLineOverlay.Graphics.Add(new Graphic(geometry));
                    }
                    
                    await UpdateDrawNeighborsAsync(geometryBuilder.ToGeometry(), this.GeographyNeighbors);

                    if (RequestedGeometryType == GeometryType.Point)
                    {
                        //stop collection on first point collected
                        if (SaveAreaCommand.CanExecute())
                            await SaveAreaCommand.ExecuteAsync();                    
                    }

                    var collectedPoints = geometryBuilder.PointCount;
                    this.CanSave = RequestedGeometryType switch {
                            GeometryType.Polygon => collectedPoints > 2,
                            GeometryType.Polyline => collectedPoints > 2,
                            GeometryType.Point => collectedPoints  > 0,
                            GeometryType.Multipoint => collectedPoints > 1,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                    if (RequestedGeometryType == GeometryType.Polygon && !IsManual)
                    {
                        HasWarning = !geometryBuilder.IsCorrectlyMeasured(RequestedAccuracy);
                    }

                    this.CanUndo =
                        (RequestedGeometryInputMode == GeometryInputMode.Semiautomatic && collectedPoints > 0);
                    
                    VibrationService.Vibrate();
                }
            }
            catch (Exception e)
            {
                logger.Error("Error on adding point", e);
                throw;
            }
        }

        private string warning;
        public string Warning
        {
            get => this.warning;
            set => this.RaiseAndSetIfChanged(ref this.warning, value);
        }
 
        private bool isWarningVisible;
        public bool IsWarningVisible
        {
            get => this.isWarningVisible;
            set => this.RaiseAndSetIfChanged(ref this.isWarningVisible, value);
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

        private string startButtonText;
        public string StartButtonText
        {
            get => this.startButtonText;
            set => this.RaiseAndSetIfChanged(ref this.startButtonText, value);
        }

        private bool isEditing;
        public bool IsEditing
        {
            get => this.isEditing;
            set => this.RaiseAndSetIfChanged(ref this.isEditing, value);
        }

        private bool isGeometryAreaVisible;
        public bool IsGeometryAreaVisible
        {
            get => this.isGeometryAreaVisible;
            set => this.RaiseAndSetIfChanged(ref this.isGeometryAreaVisible, value);
        }

        private bool hasWarning;
        public bool HasWarning
        {
            get => this.hasWarning;
            set => this.RaiseAndSetIfChanged(ref this.hasWarning, value);
        }

        private bool isGeometryLengthVisible;
        public bool IsGeometryLengthVisible
        {
            get => this.isGeometryLengthVisible;
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

        private bool canStartStopCollecting;
        public bool CanStartStopCollecting
        {
            get => this.canStartStopCollecting;
            set => this.RaiseAndSetIfChanged(ref this.canStartStopCollecting, value);
        }
        
        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.RaiseAndSetIfChanged(ref this.isInProgress, value);
        }

        private bool canAddPoint;
        public bool CanAddPoint
        {
            get => this.canAddPoint;
            set => this.RaiseAndSetIfChanged(ref this.canAddPoint, value);
        }

        private bool addPointVisible;
        public bool AddPointVisible
        {
            get => this.addPointVisible;
            set => this.RaiseAndSetIfChanged(ref this.addPointVisible, value);
        }

        private bool startButtonVisible;
        public bool StartButtonVisible
        {
            get => this.startButtonVisible;
            set => this.RaiseAndSetIfChanged(ref this.startButtonVisible, value);
        }        

        private bool isPanelVisible;
        public bool IsPanelVisible
        {
            get => this.isPanelVisible;
            set => this.RaiseAndSetIfChanged(ref this.isPanelVisible, value);
        }

        public IMvxCommand SwitchPanelCommand => new MvxCommand(() =>
        {
            IsPanelVisible = !IsPanelVisible;
        });
        
        public IMvxAsyncCommand<MapDescription> SwitchMapCommand => new MvxAsyncCommand<MapDescription>(async (mapDescription) =>
        {
            this.SelectedMap = mapDescription.MapName;
            IsPanelVisible = false;

            await this.UpdateBaseMap();
            var geometry = this.MapView.SketchEditor?.Geometry;

            //update internal structures
            //SpatialReference of new map could differ from initial
            if (geometry != null)
            {
                if (this.MapView != null && !this.MapView.Map.SpatialReference.IsEqual(geometry.SpatialReference))
                    geometry = GeometryEngine.Project(geometry, this.MapView.Map.SpatialReference);

                this.MapView?.SketchEditor.ClearGeometry();
                this.MapView?.SketchEditor.ReplaceGeometry(geometry);
                
                await DrawNeighborsAsync(geometry).ConfigureAwait(false);
            }
        });
        
        public override void ViewDisappearing()
        {
            //locationDataSource.LocationChanged -= LocationDataSourceOnLocationChanged;
            base.ViewDisappearing();
        }

        protected override async Task SetViewToValues()
        {
            Geometry currentGeometry = IsManual ? this.MapView.SketchEditor?.Geometry : geometryBuilder?.ToGeometry();
            await SetViewpointToGeometry(currentGeometry);
        }
        
        
        protected async Task SetViewpointToGeometry(Geometry geometry)
        {
            if (geometry != null)
            {
                if (geometry is MapPoint point)
                    await this.MapView.SetViewpointCenterAsync(point).ConfigureAwait(false);
                else if (geometry.Extent != null 
                         && geometry.Extent.Height != 0 
                         && geometry.Extent.Width != 0
                         && !geometry.Extent.IsEmpty)
                {
                    await this.MapView.SetViewpointGeometryAsync(geometry, 120).ConfigureAwait(false);
                }
            }
        }

        public override void Dispose()
        {
            if (locationDataSource != null)
            {
                locationDataSource.LocationChanged -= LocationDataSourceOnLocationChanged;
                locationDataSource.StopAsync();
            }

            collectionCancellationTokenSource?.Cancel();
            collectionCancellationTokenSource?.Dispose();
            collectionCancellationTokenSource = null;

            base.Dispose();
        }
    }
}

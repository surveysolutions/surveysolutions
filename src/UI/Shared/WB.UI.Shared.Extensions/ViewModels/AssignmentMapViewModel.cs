using System.ComponentModel;
using System.Drawing;
using Android.Content;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Kotlin.Reflect;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Attributes;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.Services;
using WB.UI.Shared.Extensions.Views;
using Xamarin.Essentials;

namespace WB.UI.Shared.Extensions.ViewModels;

public class AssignmentMapViewModelArgs
{
    public int AssignmentId { get; set; }
}


[InterviewEntryPoint]
public class AssignmentMapViewModel: MarkersMapInteractionViewModel<AssignmentMapViewModelArgs>
{
    private AssignmentDocument assignment;

    private readonly IGeolocationBackgroundServiceManager backgroundServiceManager;
    private readonly IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage;
    private readonly IAssignmentMapSettings assignmentMapSettings;
    private readonly IPlainStorage<GeoTrackingPoint, int?> geoTrackingPointsStorage;
    
    private readonly IGeoTrackingListener geoTrackingListener;
    private readonly IGeofencingListener geofencingListener;

    public AssignmentMapViewModel(IPrincipal principal, 
        IViewModelNavigationService viewModelNavigationService,
        IUserInteractionService userInteractionService,
        IMapService mapService,
        IEnumeratorSettings enumeratorSettings,
        ILogger logger,
        IMapUtilityService mapUtilityService,
        IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher,
        IPermissionsService permissionsService,
        IEnumeratorSettings settings,
        IGeolocationBackgroundServiceManager backgroundServiceManager,
        IDashboardViewModelFactory dashboardViewModelFactory,
        IAssignmentDocumentsStorage assignmentsRepository,
        IPlainStorage<InterviewView> interviewViewRepository,
        IGeoTrackingListener geoTrackingListener,
        IGeofencingListener geofencingListener, 
        IPlainStorage<GeoTrackingPoint, int?> geoTrackingPointsStorage, 
        IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage,
        IAssignmentMapSettings assignmentMapSettings) 
        : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, 
               enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher, permissionsService, 
               settings, dashboardViewModelFactory, assignmentsRepository, interviewViewRepository)
    {
        this.backgroundServiceManager = backgroundServiceManager;
        this.geoTrackingListener = geoTrackingListener;
        this.geofencingListener = geofencingListener;
        this.geoTrackingPointsStorage = geoTrackingPointsStorage;
        this.geoTrackingRecordsStorage = geoTrackingRecordsStorage;
        this.assignmentMapSettings = assignmentMapSettings;

        this.ShowInterviews = true;

        backgroundServiceManager.LocationReceived += BackgroundServiceManagerOnLocationReceived;
    }

    public bool AllowGeoTracking => assignmentMapSettings.AllowGeoTracking;
    public bool AllowGeofencing => assignmentMapSettings.AllowGeofencing;

    private GraphicsOverlayCollection graphicsOverlays = new GraphicsOverlayCollection();
    public GraphicsOverlayCollection GraphicsOverlays
    {
        get => this.graphicsOverlays;
        set => this.RaiseAndSetIfChanged(ref this.graphicsOverlays, value);
    }
    
    private bool showInterviews = true;
    public override bool ShowInterviews
    {
        get => this.showInterviews;
        set => this.RaiseAndSetIfChanged(ref this.showInterviews, value);
    }
    private bool showAssignments = false;
    public override bool ShowAssignments
    {
        get => this.showAssignments;
        set => this.RaiseAndSetIfChanged(ref this.showAssignments, value);
    }

    protected override List<AssignmentDocument> FilteredAssignments()
    {
        throw new NotImplementedException();
    }

    public override void Prepare(AssignmentMapViewModelArgs parameter)
    {
        var assignmentId = parameter.AssignmentId;

        assignment = assignmentsRepository.GetById(assignmentId);
    }
        
    public override async Task Initialize()
    {
        await base.Initialize();
        
        ReloadEntities();
            
        this.GraphicsOverlays.Add(graphicsOverlay);
    }

    public override async Task OnMapLoaded()
    {
        var targetArea = assignment.TargetArea;
        if (!string.IsNullOrWhiteSpace(targetArea))
        {
            var fullPathToShapefile = AvailableShapefiles.FirstOrDefault(sf => sf.ShapefileFileName == targetArea)?.FullPath;
            if (!string.IsNullOrWhiteSpace(fullPathToShapefile))
                await LoadShapefileByPath(fullPathToShapefile);
        }
        
        await RefreshMarkers(setViewToMarkers: true);
        
        await DrawGeoTrackingAsync();
    }

    public override void ViewAppeared()
    {
        base.ViewAppeared();
        this.MapView?.RefreshDrawableState();
    }

    public override MapDescription GetSelectedMap(MvxObservableCollection<MapDescription> mapsToSelectFrom)
    {
        MapDescription mapToLoad = null;
        
        if (!string.IsNullOrEmpty(this.LastMap))
        {
            mapToLoad = mapsToSelectFrom.FirstOrDefault(x => x.MapName == LastMap);
        }

        mapToLoad ??= mapsToSelectFrom.FirstOrDefault(x => x.MapType == MapType.LocalFile);
            
        return mapToLoad ?? mapsToSelectFrom.First();
    }
    
    protected override async Task AfterShapefileLoadedHandler()
    {
        await CheckMarkersAgainstShapefile();
    }
    
    protected override List<InterviewView> FilteredInterviews()
    {
        return Interviews.Where(i => i.Assignment == assignment.Id).ToList();
    }

    protected override async Task SetViewToValues()
    {
        Envelope graphicExtent = null;
        var geometries = graphicsOverlay.Graphics
            .Where(graphic => graphic.Geometry != null && !graphic.Geometry.IsEmpty)
            .Select(graphic => graphic.Geometry)
            .ToList();
        if (geometries.Count > 0)
        {
            EnvelopeBuilder eb = new EnvelopeBuilder(GeometryEngine.CombineExtents(geometries));
            eb.Expand(1.2);
            graphicExtent = eb.Extent;
        }

        if (graphicExtent != null)
        {
            await MapView.SetViewpointAsync(new Viewpoint(graphicExtent), TimeSpan.FromSeconds(4));
        }
    }

    public IMvxCommand ShowTrackingDataCommand => new MvxAsyncCommand(SetViewToTrackingValues);
    
    protected async Task SetViewToTrackingValues()
    {
        var existedLayer = this.MapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == GeoTrackingLayerName);
        if (existedLayer == null)
            return;
        
        var featureCollectionLayer = (FeatureCollectionLayer)existedLayer;
        var featureCollectionTables = featureCollectionLayer.FeatureCollection.Tables;
        var geoTrackingFeatureCollectionTable = featureCollectionTables[0];
        
        var points = geoTrackingFeatureCollectionTable
            .Where(f => (f.Geometry as Polyline)?.Parts.Count > 0)
            .SelectMany(f => (f.Geometry as Polyline)?.Parts.SelectMany(p => p.Points))
            .ToList();

        if (points.Count > 0)
        {
            EnvelopeBuilder eb = new EnvelopeBuilder(GeometryEngine.CombineExtents(points));
            eb.Expand(1.2);

            await MapView.SetViewpointAsync(new Viewpoint(eb.Extent), TimeSpan.FromSeconds(4));
        }
    }
    

    public IMvxAsyncCommand SwitchMapCommand => new MvxAsyncCommand(async () =>
    {
        var options = AvailableMaps.Select(m => new BottomSheetOption()
        {
            Name = m.MapName,
            Value = m.MapName,
            IsSelected = m.MapName == SelectedMap
        }).ToArray();
        var args = new BottomSheetOptionsSelectorViewModelArgs()
        {
            Title = UIResources.SelectMapTitle,
            Options = options,
            Callback = async (option) =>
            {
                if (SelectedMap != option.Value)
                {
                    await UpdateBaseMap(option.Value);
                    
                    await this.ShowFullMapCommand.ExecuteAsync(null);
                }
            }
        };
        await NavigationService.Navigate<BottomSheetOptionsSelectorViewModel, BottomSheetOptionsSelectorViewModelArgs>(args);
        
        //await this.UpdateBaseMap(mapDescription.MapName);
    });

    public IMvxCommand SwitchPanelCommand => new MvxCommand(() =>
    {
        IsPanelVisible = !IsPanelVisible;
    });

    private bool isPanelVisible;
    public bool IsPanelVisible
    {
        get => this.isPanelVisible;
        set => this.RaiseAndSetIfChanged(ref this.isPanelVisible, value);
    }
    
    public IMvxAsyncCommand NavigateToDashboardCommand => 
        new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToDashboardAsync());

    public IMvxAsyncCommand CreateInterviewCommand => 
        new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToCreateAndLoadInterview(assignment.Id));

    private bool isEnabledGeofencing;
    public bool IsEnabledGeofencing
    {
        get => this.isEnabledGeofencing;
        set => this.RaiseAndSetIfChanged(ref this.isEnabledGeofencing, value);
    }

    public IMvxAsyncCommand StartGeofencingCommand => new MvxAsyncCommand(ToggleGeofencingService);
    
    private async Task ToggleGeofencingService()
    {
        if (LoadedShapefile == null)
            return;
        
        try
        {
            if (!IsEnabledGeofencing)
            {
                await permissionsService.AssureHasPermissionOrThrow<Permissions.LocationAlways>();

                this.geofencingListener.Start(LoadedShapefile);
                await this.backgroundServiceManager.StartListen(geofencingListener);
                
                await SwitchLocator();
            }
            else
            {
                this.backgroundServiceManager.StopListen(geofencingListener);
                this.IsWarningVisible = false;
            }

            IsEnabledGeofencing = !IsEnabledGeofencing;
        }
        catch (MissingPermissionsException mp) when (mp.PermissionType == typeof(Permissions.LocationAlways))
        {
            this.UserInteractionService.ShowToast(UIResources.MissingPermissions_MapsLocation);
            return;
        }
        catch (Exception exc)
        {
            logger.Error("Error occurred on map location start.", exc);
        }        
    }
    
    
    private bool isEnabledGeoTracking;
    public bool IsEnabledGeoTracking
    {
        get => this.isEnabledGeoTracking;
        set => this.RaiseAndSetIfChanged(ref this.isEnabledGeoTracking, value);
    }
    
    public IMvxAsyncCommand StartGeoTrackingCommand => new MvxAsyncCommand(ToggleGeoTrackingService);
    
    private async Task ToggleGeoTrackingService()
    {
        try
        {
            if (!IsEnabledGeoTracking)
            {
                await permissionsService.AssureHasPermissionOrThrow<Permissions.LocationAlways>();

                await SwitchLocator();

                this.geoTrackingListener.Start(assignment.Id);
                await this.backgroundServiceManager.StartListen(geoTrackingListener);
            }
            else
            {
                this.geoTrackingListener.Stop();
                this.backgroundServiceManager.StopListen(geoTrackingListener);
            }

            IsEnabledGeoTracking = !IsEnabledGeoTracking;
        }
        catch (MissingPermissionsException mp) when (mp.PermissionType == typeof(Permissions.LocationAlways))
        {
            this.UserInteractionService.ShowToast(UIResources.MissingPermissions_MapsLocation);
            return;
        }
        catch (Exception exc)
        {
            logger.Error("Error occurred on map location start.", exc);
        }        
    }

    private async void BackgroundServiceManagerOnLocationReceived(object sender, LocationReceivedEventArgs e)
    {
        LogTestRecords(e);

        await UpdateGeoTrackingPointsAsync(e.Location);
    }

    private List<RecordWithPoints> GeoTrackingRecords { get; set; }
    
    public const string GeoTrackingLayerName = "GeoTrackingLayer";

    private Feature lastGeoTrackingFeature;

    private class RecordWithPoints
    {
        public int RecordId { get; set; }
        public List<GeoTrackingPoint> Points { get; set; }
    }
    
    private async Task DrawGeoTrackingAsync()
    {
        //GeoTrackingRecords = new List<RecordWithPoints>();
        
        GeoTrackingRecords = geoTrackingRecordsStorage
                .WhereSelect(where => where.AssignmentId == assignment.Id,
                    r =>
                    new RecordWithPoints()
                    {
                        RecordId = r.Id.Value,
                        Points = geoTrackingPointsStorage.Where(p => p.GeoTrackingRecordId == r.Id.Value).ToList()
                    }
        ).ToList();
        
        GeoTrackingRecords.Add(new RecordWithPoints() { Points = new List<GeoTrackingPoint>()});
        
        if(this.MapView?.Map?.SpatialReference == null)
            return;
        
        var esriGeometryType = GeometryType.Polyline;

        IEnumerable<Field> fields = new Field[]
        {
            new Field(FieldType.Text, "name", "Name", 50)
        };
        var mapViewSpatialReference = this.MapView.Map.SpatialReference;
        var geoTrackingFeatureCollectionTable = new FeatureCollectionTable(fields, esriGeometryType, mapViewSpatialReference)
            {
                Renderer = CreateRenderer()
            };

        for (int i = 0; i < GeoTrackingRecords.Count; i++)
        {
            var record = GeoTrackingRecords[i];

            var mapPoints = new List<MapPoint>();
            foreach (var point in record.Points)
            {
                var mapPoint = new MapPoint(point.Longitude, point.Latitude, SpatialReferences.Wgs84);
                mapPoints.Add(mapPoint);
            }
            
            var feature = geoTrackingFeatureCollectionTable.CreateFeature();
            Geometry multipoint = new Polyline(mapPoints, SpatialReferences.Wgs84);
            if (multipoint.SpatialReference != Map.SpatialReference)
                multipoint = multipoint.Project(Map.SpatialReference);
            
            feature.Geometry = multipoint;
            await geoTrackingFeatureCollectionTable.AddFeatureAsync(feature);

            this.lastGeoTrackingFeature = feature;
        }

        FeatureCollection featuresCollection = new FeatureCollection();
        featuresCollection.Tables.Add(geoTrackingFeatureCollectionTable);
        FeatureCollectionLayer featureCollectionLayer = new FeatureCollectionLayer(featuresCollection)
        {
            Name = GeoTrackingLayerName
        };

        var existedLayer = this.MapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == featureCollectionLayer.Name);
        if (existedLayer != null)
            this.MapView.Map.OperationalLayers.Remove(existedLayer);
        
        this.MapView.Map.OperationalLayers.Add(featureCollectionLayer);
    }

    private object geoTrackingLayerLock = new object();
    
    private async Task UpdateGeoTrackingPointsAsync(GpsLocation location)
    {
        if (GeoTrackingRecords == null || GeoTrackingRecords.Count == 0)
            return;

        var existedLayer = this.MapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == GeoTrackingLayerName);
        if (existedLayer == null)
            return;
        
        var featureCollectionLayer = (FeatureCollectionLayer)existedLayer;
        var featureCollectionTables = featureCollectionLayer.FeatureCollection.Tables;
        var geoTrackingFeatureCollectionTable = featureCollectionTables[0];
        var lastRecord = GeoTrackingRecords.Last();
        lastRecord.Points.Add(new GeoTrackingPoint()
        {
            GeoTrackingRecordId = lastRecord.RecordId,
            Longitude = location.Longitude,
            Latitude = location.Latitude,
        });

        try
        {
            Monitor.Enter(geoTrackingLayerLock);

            var polyline = (Polyline)lastGeoTrackingFeature.Geometry;
            List<MapPoint> points = new List<MapPoint>(polyline.Parts.SelectMany(l => l.Points));
            MapPoint mapPoint = new MapPoint(location.Longitude, location.Latitude, SpatialReferences.Wgs84);
            if (mapPoint.SpatialReference != Map.SpatialReference)
                mapPoint = (MapPoint)mapPoint.Project(Map.SpatialReference);
            points.Add(mapPoint);
            Geometry newGeometry = new Polyline(points);

            var feature = geoTrackingFeatureCollectionTable.CreateFeature();
            feature.Geometry = newGeometry;
            await geoTrackingFeatureCollectionTable.AddFeatureAsync(feature);
            await geoTrackingFeatureCollectionTable.DeleteFeatureAsync(lastGeoTrackingFeature);
            lastGeoTrackingFeature = feature;
        }
        catch(Exception)
        {
            // ignore
        }
        finally
        {
            Monitor.Exit(geoTrackingLayerLock);
        }
    }

    private Renderer CreateRenderer()
    {
        var outerLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.White, 5);
        var innerLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(140, 89, 222), 2);
        //var innerLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);
        var sym = new CompositeSymbol(new List<Symbol> { outerLineSymbol, innerLineSymbol });
        //GeometryType.Multipoint => new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, color, 18),
        //GeometryType.Polyline => new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, color, 2),
        return new SimpleRenderer(sym);
    }

    private void LogTestRecords(LocationReceivedEventArgs e)
    {
        var loc = $"{e.Location.Latitude}  {e.Location.Longitude}";

        if (geofencingListener.LastResult?.OutShapefile ?? false)
        {
            IsWarningVisible = true;
            Warning = UIResources.Geofencing_Warning_Message;
        }
        else
        {
            IsWarningVisible = false;
        }
    }


    public override void Dispose()
    {
        backgroundServiceManager.LocationReceived -= BackgroundServiceManagerOnLocationReceived;
        
        if (geoTrackingListener != null)
            this.backgroundServiceManager.StopListen(geoTrackingListener);
        if (geofencingListener != null)
            this.backgroundServiceManager.StopListen(geofencingListener);

        base.Dispose();
    }
}

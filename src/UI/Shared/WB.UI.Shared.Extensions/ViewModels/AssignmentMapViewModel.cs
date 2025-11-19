using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Android.Content;
using AndroidX.Annotations;
using Esri.ArcGISRuntime;
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
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
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
    private readonly Guid vmId = Guid.NewGuid();
    private static bool lastGeoFencingStateOnInterviewCreation = false;
    private static bool lastGeoTrackingStateOnInterviewCreation = false;
    
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
               dashboardViewModelFactory, assignmentsRepository, interviewViewRepository)
    {
        this.backgroundServiceManager = backgroundServiceManager;
        this.geoTrackingListener = geoTrackingListener;
        this.geofencingListener = geofencingListener;
        this.geoTrackingPointsStorage = geoTrackingPointsStorage;
        this.geoTrackingRecordsStorage = geoTrackingRecordsStorage;
        this.assignmentMapSettings = assignmentMapSettings;

        this.ShowInterviews = true;
        
        //check backgroundServiceManager and stop any collection if it is running
        //when a new instance of AssignmentMapViewModel is created
        backgroundServiceManager.StopAll();
    }

    public bool IsGeoTrackingPemitted => assignmentMapSettings.AllowGeoTracking;
    public bool IsGeofencingPermitted => assignmentMapSettings.AllowGeofencing;
    public bool IsCreateInterviewPermitted => assignmentMapSettings.AllowCreateInterview;
    
    private bool isGeofencingAvailable = true;
    public bool IsGeofencingAvailable
    {
        get => this.isGeofencingAvailable;
        set => this.RaiseAndSetIfChanged(ref this.isGeofencingAvailable, value);
    }
    
    private bool isGeoTrackingAvailable = true;
    public bool IsGeoTrackingAvailable
    {
        get => this.isGeoTrackingAvailable;
        set => this.RaiseAndSetIfChanged(ref this.isGeoTrackingAvailable, value);
    }

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

    //do not restart services if the activity was killed
    
    protected override void SaveStateToBundle(IMvxBundle bundle)
    {
    //     isBackground = true;
    //     
         base.SaveStateToBundle(bundle);
         
         bundle.Data["assignmentId"] = assignment.Id.ToString();
    //     
    //     backgroundServiceManager.LocationReceived -= BackgroundServiceManagerOnLocationReceived;
    }
    
    protected override void ReloadFromBundle(IMvxBundle state)
    {
    //     isBackground = false;
         
        base.ReloadFromBundle(state);
         
        if (state.Data.TryGetValue("assignmentId", out var assignmentIdStr) && int.TryParse(assignmentIdStr, out var assignmentId))
        {
            assignment = assignmentsRepository.GetById(assignmentId);
        }
    
    //     var geofencing = backgroundServiceManager.GetListen(geofencingListener);
    //     var geotracking = backgroundServiceManager.GetListen(geoTrackingListener);
    //
    //     if (geofencing != null || geotracking != null)
    //     {
    //         await SwitchLocator();
    //     }
    //
    //     if (geofencing != null)
    //     {
    //         var geolocationListener = (IGeofencingListener)geofencing;
    //         this.geofencingListener.Init(geolocationListener.Shapefile);
    //         await this.backgroundServiceManager.StartListen(geofencingListener);
    //
    //         IsEnabledGeofencing = true;
    //     }
    //     
    //     if (geotracking != null)
    //     {
    //         this.geoTrackingListener.Init(assignment.Id);
    //         await this.backgroundServiceManager.StartListen(geoTrackingListener);
    //         
    //         IsEnabledGeoTracking = true;
    //     }
    //     
    //     backgroundServiceManager.LocationReceived += BackgroundServiceManagerOnLocationReceived;
    }
        
    public override async Task Initialize()
    {
        await base.Initialize();
        
        ReloadEntities();
            
        this.GraphicsOverlays.Add(graphicsOverlay);

        CheckExistingOfGpsProvider();

        IsGeofencingAvailable = CanStartGeofencing();
        IsGeoTrackingAvailable = backgroundServiceManager.HasGpsProvider();
    }
    
    public override async void ViewAppeared()
    {
        base.ViewAppeared();
        this.MapView?.RefreshDrawableState();
        
        backgroundServiceManager.LocationReceived += BackgroundServiceManagerOnLocationReceived;
        
        await DrawGeoTrackingAsync();
        if (!reminderWasShown && (lastGeoFencingStateOnInterviewCreation || lastGeoTrackingStateOnInterviewCreation))
        {
            await this.UserInteractionService.AlertAsync(
                EnumeratorUIResources.AssigmentMap_GeoTrackingReminder,
                okButton: UIResources.Ok);
            
            reminderWasShown = true;
        }
    }
    public override void ViewDisappeared()
    {
        base.ViewDisappeared();
        
        backgroundServiceManager.LocationReceived -= BackgroundServiceManagerOnLocationReceived;
    }

    private void CheckExistingOfGpsProvider()
    {
        var hasGpsProvider = backgroundServiceManager.HasGpsProvider();
        if (!hasGpsProvider)
        {
            Warning = EnumeratorUIResources.Error_NoGpsProvider;
            IsWarningVisible = true;
        }
    }

    public override async Task OnMapLoaded()
    {
        var targetArea = assignment.TargetArea;
        if (!string.IsNullOrWhiteSpace(targetArea))
        {
            var fullPathToShapefile = AvailableShapefiles.FirstOrDefault(sf => sf.ShapefileFileName == targetArea)?.FullPath;
            if (!string.IsNullOrWhiteSpace(fullPathToShapefile))
                await LoadShapefileByPath(fullPathToShapefile);

            if (LoadedShapefile == null)
            {
                Warning = EnumeratorUIResources.Error_NoTargetAreaShapefile;
                IsWarningVisible = true;
            }
        }
        
        await RefreshMarkers(setViewToMarkers: true);
        
        await DrawGeoTrackingAsync();
    }

    private bool reminderWasShown = false; 
    
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
    
    protected override Task AfterShapefileLoadedHandler()
    {
        IsGeofencingAvailable = CanStartGeofencing();
        return Task.CompletedTask;
    }

    protected override Task CheckMarkersAgainstShapefile()
    {
        return Task.CompletedTask;
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
            await MapView.SetViewpointAsync(new Viewpoint(graphicExtent), TimeSpan.FromSeconds(1));
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

            await MapView.SetViewpointAsync(new Viewpoint(eb.Extent), TimeSpan.FromSeconds(1));
        }
    }
    
    public IMvxAsyncCommand NavigateToDashboardCommand => 
        new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToDashboardAsync());

    public IMvxAsyncCommand CreateInterviewCommand => 
        new MvxAsyncCommand(async () =>
        {
            if (IsEnabledGeofencing)
            {
                if (geofencingListener.LastResult?.OutShapefile ?? true)
                {
                    var confirmResult = await UserInteractionService.ConfirmAsync(
                        EnumeratorUIResources.AssigmentMap_CreateInterview_OutTargetArea_Warning);
                    if (!confirmResult)
                        return;
                }
            }
            else
            {
                var confirmResult = await UserInteractionService.ConfirmAsync(
                    EnumeratorUIResources.AssigmentMap_CreateInterview_DisabledTargetArea_Warning);
                if (!confirmResult)
                    return;
            }
            
            lastGeoFencingStateOnInterviewCreation = isEnabledGeofencing;
            lastGeoTrackingStateOnInterviewCreation = isEnabledGeoTracking;

            await this.ViewModelNavigationService.NavigateToCreateAndLoadInterview(assignment.Id);
        }, CanCreateInterview
    );

    private bool CanCreateInterview()
    {
        return AllowCreateInterview && 
               (!assignment.Quantity.HasValue || Math.Max(val1: 0, val2: InterviewsLeftByAssignmentCount) > 0);
    }
    
    public bool AllowCreateInterview => assignmentMapSettings.AllowCreateInterview;

    private int InterviewsLeftByAssignmentCount =>
        assignment.Quantity.GetValueOrDefault() - (assignment.CreatedInterviewsCount ?? 0);

    protected override void ReloadEntities()
    {
        base.ReloadEntities();

        this.assignment = assignmentsRepository.GetById(assignment.Id);
    }

    private bool isEnabledGeofencing;
    public bool IsEnabledGeofencing
    {
        get => this.isEnabledGeofencing;
        set => this.RaiseAndSetIfChanged(ref this.isEnabledGeofencing, value);
    }

    private bool CanStartGeofencing() => LoadedShapefile != null && backgroundServiceManager.HasGpsProvider();

    public IMvxAsyncCommand StartGeofencingCommand => new MvxAsyncCommand(ToggleGeofencingService);
    
    private async Task ToggleGeofencingService()
    {
        if (!CanStartGeofencing())
            return;
        
        try
        {
            if (!IsEnabledGeofencing)
            {
                await permissionsService.AssureHasPermissionOrThrow<Permissions.LocationAlways>();
                this.geofencingListener.Init(LoadedShapefile);
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
        if (!backgroundServiceManager.HasGpsProvider())
            return;

        try
        {
            if (!IsEnabledGeoTracking)
            {
                await permissionsService.AssureHasPermissionOrThrow<Permissions.LocationAlways>();
                lastRecordWithPoints = new RecordWithPoints() { Points = new List<GeoTrackingPoint>()};
                this.geoTrackingListener.Init(assignment.Id);
                await this.backgroundServiceManager.StartListen(geoTrackingListener);

                await SwitchLocator();
            }
            else
            {
                this.backgroundServiceManager.StopListen(geoTrackingListener);
                this.geoTrackingListener.Stop();
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
        ShowGeofencingWarningIfNeed(e);
        await UpdateGeoTrackingPointsAsync(e.Location);
    }

    public const string GeoTrackingLayerName = "GeoTrackingLayer";

    private Feature currentTrackFeature;

    RecordWithPoints lastRecordWithPoints = new RecordWithPoints() { Points = new List<GeoTrackingPoint>()};

    private class RecordWithPoints
    {
        public List<GeoTrackingPoint> Points { get; set; }
    }
    private bool isDrawingGeoTracking = false;
    private async Task DrawGeoTrackingAsync()
    {
        if(MapView?.Map?.LoadStatus != LoadStatus.Loaded)
            return;
        if(isDrawingGeoTracking)
            return;
        if(assignment == null)
            return;
        
        try
        {
            isDrawingGeoTracking = true;
            var geoTrackingRecords = geoTrackingRecordsStorage
                .WhereSelect(where => where.AssignmentId == assignment.Id,
                    r =>
                    new RecordWithPoints()
                    {
                        Points = geoTrackingPointsStorage.Where(p => p.GeoTrackingRecordId == r.Id.Value).ToList()
                    }
            ).ToList();
        
            if(this.MapView?.Map?.SpatialReference == null)
                return;
        
            IEnumerable<Field> fields = new Field[]
            {
                new Field(FieldType.Text, "name", "Name", 50)
            };
            var mapViewSpatialReference = this.MapView.Map.SpatialReference;
            var geoTrackingPolylineFeatureCollectionTable = new FeatureCollectionTable(fields, GeometryType.Polyline, mapViewSpatialReference)
            {
                Renderer = CreateGeoTrackingRenderer(GeometryType.Polyline)
            };
            var geoTrackingPointFeatureCollectionTable = new FeatureCollectionTable(fields, GeometryType.Point, mapViewSpatialReference)
            {
                Renderer = CreateGeoTrackingRenderer(GeometryType.Point)
            };

            for (int i = 0; i < geoTrackingRecords.Count; i++)
            {
                var record = geoTrackingRecords[i];

                var mapPoints = new List<MapPoint>();
                foreach (var point in record.Points)
                {
                    var mapPoint = new MapPoint(point.Longitude, point.Latitude, SpatialReferences.Wgs84);
                    mapPoints.Add(mapPoint);
                }

                if (mapPoints.Count == 1)
                {
                    var feature = geoTrackingPointFeatureCollectionTable.CreateFeature();
                    Geometry point = new MapPoint(mapPoints[0].X, mapPoints[0].Y, SpatialReferences.Wgs84);
                    if (point.SpatialReference != Map.SpatialReference)
                        point = point.Project(Map.SpatialReference);

                    feature.Geometry = point;
                    await geoTrackingPointFeatureCollectionTable.AddFeatureAsync(feature);
                }
                else if (mapPoints.Count > 1)
                {
                    var feature = geoTrackingPolylineFeatureCollectionTable.CreateFeature();
                    Geometry polyline = new Polyline(mapPoints, SpatialReferences.Wgs84);
                    if (polyline.SpatialReference != Map.SpatialReference)
                        polyline = polyline.Project(Map.SpatialReference);

                    feature.Geometry = polyline;
                    await geoTrackingPolylineFeatureCollectionTable.AddFeatureAsync(feature);
                }
            }

            FeatureCollection featuresCollection = new FeatureCollection();
            featuresCollection.Tables.Add(geoTrackingPolylineFeatureCollectionTable);
            featuresCollection.Tables.Add(geoTrackingPointFeatureCollectionTable);
            FeatureCollectionLayer featureCollectionLayer = new FeatureCollectionLayer(featuresCollection)
            {
                Name = GeoTrackingLayerName
            };

            var existedLayer = this.MapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == featureCollectionLayer.Name);
            if (existedLayer != null)
            {
                this.MapView.Map.OperationalLayers.Remove(existedLayer);
                // Invalidate the current track feature since we're recreating the layer
                this.currentTrackFeature = null;
            }
        
            this.MapView.Map.OperationalLayers.Add(featureCollectionLayer);

        }
        finally
        {
            isDrawingGeoTracking = false;
        }
    }

    private object geoTrackingLayerLock = new object();

    private bool isUpdating = false;
    
    private async Task UpdateGeoTrackingPointsAsync(GpsLocation location)
    {
        if (!IsEnabledGeoTracking || lastRecordWithPoints?.Points == null)
            return;

        var geoTrackingLayer = this.MapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == GeoTrackingLayerName);
        if (geoTrackingLayer == null)
            return;
        
        lastRecordWithPoints.Points.Add(new GeoTrackingPoint()
        {
            Longitude = location.Longitude,
            Latitude = location.Latitude,
        });

        if(isUpdating)
           return;
        try
        {
            isUpdating = true;

            var featureCollectionLayer = (FeatureCollectionLayer)geoTrackingLayer;
            var featureCollectionTables = featureCollectionLayer.FeatureCollection.Tables;

            if (lastRecordWithPoints.Points.Count == 1)
            {
                var geoTrackingPointFeatureCollectionTable = featureCollectionTables[1];
                var updatedTrackFeature = geoTrackingPointFeatureCollectionTable.CreateFeature();

                Geometry point = new MapPoint(location.Longitude, location.Latitude, SpatialReferences.Wgs84);
                if (point.SpatialReference != Map.SpatialReference)
                    point = point.Project(Map.SpatialReference);

                updatedTrackFeature.Geometry = point;
                await geoTrackingPointFeatureCollectionTable.AddFeatureAsync(updatedTrackFeature);

                this.currentTrackFeature = updatedTrackFeature;
            }
            else if (lastRecordWithPoints.Points.Count > 1)
            {
                var mapPoints = lastRecordWithPoints.Points.Select(point =>
                        new MapPoint(point.Longitude, point.Latitude, SpatialReferences.Wgs84))
                    .ToList();

                //IsWarningVisible = true;
                //Warning = this.Map.

                var geoTrackingPolylineFeatureCollectionTable = featureCollectionTables[0];
                var updatedTrackFeature = geoTrackingPolylineFeatureCollectionTable.CreateFeature();
                Geometry polyline = new Polyline(mapPoints, SpatialReferences.Wgs84);
                if (polyline.SpatialReference != Map.SpatialReference)
                    polyline = polyline.Project(Map.SpatialReference);

                updatedTrackFeature.Geometry = polyline;

                // Only delete if the feature reference is still valid
                if (currentTrackFeature != null && currentTrackFeature.FeatureTable != null)
                {
                    try
                    {
                        if (currentTrackFeature.FeatureTable.GeometryType == GeometryType.Point)
                            await featureCollectionTables[1].DeleteFeatureAsync(currentTrackFeature);
                        else
                            await featureCollectionTables[0].DeleteFeatureAsync(currentTrackFeature);
                    }
                    catch (ArcGISRuntimeException ex)
                    {
                        // Feature already deleted (e.g., layer was recreated), safe to ignore
                        logger.Debug("Feature already deleted during geo-tracking update", ex);
                    }
                }

                await geoTrackingPolylineFeatureCollectionTable.AddFeatureAsync(updatedTrackFeature);

                currentTrackFeature = updatedTrackFeature;
            }
        }
        finally
        {
            isUpdating = false;
        }
    }

    private Renderer CreateGeoTrackingRenderer(GeometryType geometryType)
    {
        if (geometryType == GeometryType.Polyline)
        {
            var outerLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.White, 5);
            var innerLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(40, 120, 190), 2);
            var sym = new CompositeSymbol(new List<Symbol> { outerLineSymbol, innerLineSymbol });
            return new SimpleRenderer(sym);
        }
        if (geometryType == GeometryType.Point)
        {
            var outerMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.White, 10);
            var innerMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.FromArgb(40, 120, 190), 6);
            var compositeSymbol = new CompositeSymbol(new List<Symbol> { outerMarkerSymbol, innerMarkerSymbol });
            return new SimpleRenderer(compositeSymbol);
        }
        throw new Exception("Unknown geometry type " + geometryType);
    }

    private void ShowGeofencingWarningIfNeed(LocationReceivedEventArgs e)
    {
        if (!IsEnabledGeofencing)
            return;
        
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

    private bool isBackground = false;

    public override void Dispose()
    {
        StopGeoServices();
        
        base.Dispose();
    }

    private void StopGeoServices()
    {
        if (isBackground)
            return;
        
        backgroundServiceManager.LocationReceived -= BackgroundServiceManagerOnLocationReceived;

        if (geoTrackingListener != null)
            backgroundServiceManager.StopListen(geoTrackingListener);

        if (geofencingListener != null)
            backgroundServiceManager.StopListen(geofencingListener);
    }
}

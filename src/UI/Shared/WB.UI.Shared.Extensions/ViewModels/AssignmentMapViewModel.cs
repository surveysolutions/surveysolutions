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
        IPlainStorage<GeoTrackingRecord, int?> geoTrackingRecordsStorage) 
        : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, 
               enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher, permissionsService, 
               settings, dashboardViewModelFactory, assignmentsRepository, interviewViewRepository)
    {
        this.backgroundServiceManager = backgroundServiceManager;
        this.geoTrackingListener = geoTrackingListener;
        this.geofencingListener = geofencingListener;
        this.geoTrackingPointsStorage = geoTrackingPointsStorage;
        this.geoTrackingRecordsStorage = geoTrackingRecordsStorage;

        this.ShowInterviews = true;

        this.Warning = "This is warning";
        this.IsWarningVisible = true;
        
        backgroundServiceManager.LocationReceived += BackgroundServiceManagerOnLocationReceived;
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
    

    public IMvxAsyncCommand<MapDescription> SwitchMapCommand => new MvxAsyncCommand<MapDescription>(async (mapDescription) =>
    {
        IsPanelVisible = false;
        await this.UpdateBaseMap(mapDescription.MapName);
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

    public IMvxCommand StartGeofencingCommand => 
        new MvxAsyncCommand(async () => { await ToggleGeofencingService(); }, 
            () => LoadedShapefile != null);

    private bool isGeofencingEnabled = false;
    
    private async Task ToggleGeofencingService()
    {
        try
        {
            await permissionsService.AssureHasPermissionOrThrow<Permissions.LocationAlways>();

            if (!isGeofencingEnabled)
            {
                this.geofencingListener.Start(LoadedShapefile);
                await this.backgroundServiceManager.StartListen(geofencingListener);
                
                await SwitchLocator();
            }
            else
            {
                this.backgroundServiceManager.StopListen(geofencingListener);
            }

            isGeofencingEnabled = !isGeofencingEnabled;
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
    
    public IMvxCommand StartGeoTrackingCommand => 
        new MvxAsyncCommand(async () => { await ToggleGeoTrackingService(); }, 
            () => LoadedShapefile != null);

    private bool isGeoTrackingEnabled = false;
    
    private async Task ToggleGeoTrackingService()
    {
        try
        {
            await permissionsService.AssureHasPermissionOrThrow<Permissions.LocationAlways>();

            if (!isGeoTrackingEnabled)
            {
                await DrawGeoTrackingAsync();
                await SwitchLocator();

                this.geoTrackingListener.Start(assignment.Id);
                await this.backgroundServiceManager.StartListen(geoTrackingListener);
            }
            else
            {
                this.geoTrackingListener.Stop();
                this.backgroundServiceManager.StopListen(geoTrackingListener);
            }

            isGeoTrackingEnabled = !isGeoTrackingEnabled;
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
        
        var gType = GeometryType.Multipoint;
        var esriGeometryType = GeometryType.Multipoint;

        IEnumerable<Field> fields = new Field[]
        {
            new Field(FieldType.Text, "name", "Name", 50)
        };
        var mapViewSpatialReference = this.MapView.Map.SpatialReference;
        var geoTrackingFeatureCollectionTable = new FeatureCollectionTable(fields, esriGeometryType, mapViewSpatialReference)
            {
                Renderer = CreateRenderer(gType, Color.Blue)
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
            Geometry multipoint = new Multipoint(mapPoints, SpatialReferences.Wgs84);
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

            var multipoint = (Multipoint)lastGeoTrackingFeature.Geometry;
            List<MapPoint> points = new List<MapPoint>(multipoint.Points);
            MapPoint mapPoint = new MapPoint(location.Longitude, location.Latitude, SpatialReferences.Wgs84);
            if (mapPoint.SpatialReference != Map.SpatialReference)
                mapPoint = (MapPoint)mapPoint.Project(Map.SpatialReference);
            points.Add(mapPoint);
            Geometry newGeometry = new Multipoint(points);

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

    private void LogTestRecords(LocationReceivedEventArgs e)
    {
        var loc = $"\r\n{e.Location.Latitude}  {e.Location.Longitude}";
        //geofencingViewModel.Locations = loc + geofencingViewModel.Locations;

        if (geofencingListener.LastResult?.InShapefile ?? false)
        {
            Warning = loc + " (OUT)";
        }
        else
        {
            Warning = loc + " (In)";
        }

        IsWarningVisible = true;
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

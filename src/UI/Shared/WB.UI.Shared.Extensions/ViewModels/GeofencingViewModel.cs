using System.ComponentModel;
using System.Drawing;
using Android.Content;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.Services;
using Xamarin.Essentials;

namespace WB.UI.Shared.Extensions.ViewModels;

public class GeofencingViewModelArgs
{
    public int AssignmentId { get; set; }
}


public class GeofencingViewModel: BaseMapInteractionViewModel<GeofencingViewModelArgs>
{
    private AssignmentDocument assignment;
    private string locations = "Locations: ";

    public string Locations
    {
        get => locations;
        set
        {
            if (value == locations) return;
            locations = value;
            RaisePropertyChanged(() => Locations);
        }
    }

    public IMvxCommand StartService => new MvxAsyncCommand(async () =>
    {
        await permissionsService.AssureHasPermissionOrThrow<Permissions.LocationAlways>();

        var intent = new Intent(Android.App.Application.Context, typeof(GeolocationBackgroundService));
        var componentName = Android.App.Application.Context.StartService(intent);
    });

    public IMvxCommand StopService => new MvxCommand( () =>
    {
        var intent = new Intent(Android.App.Application.Context, typeof(GeolocationBackgroundService));
        var stopService = Android.App.Application.Context.StopService(intent);
    });

    
    const string MarkerId = "marker_id";

    protected readonly IAssignmentDocumentsStorage AssignmentsRepository;
    protected readonly IPlainStorage<InterviewView> InterviewViewRepository;
    private readonly IDashboardViewModelFactory dashboardViewModelFactory;
    private readonly IVibrationService vibrationService;
    private readonly IGeolocationBackgroundServiceManager backgroundServiceManager;
    
    private IGeolocationListener geofencingListener;

    public GeofencingViewModel(IPrincipal principal, 
        IViewModelNavigationService viewModelNavigationService,
        IUserInteractionService userInteractionService,
        IMapService mapService,
        IAssignmentDocumentsStorage assignmentsRepository,
        IPlainStorage<InterviewView> interviewViewRepository,
        IEnumeratorSettings enumeratorSettings,
        ILogger logger,
        IMapUtilityService mapUtilityService,
        IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher,
        IDashboardViewModelFactory dashboardViewModelFactory, 
        IPermissionsService permissionsService,
        IEnumeratorSettings settings,
        IVibrationService vibrationService,
        IGeolocationBackgroundServiceManager backgroundServiceManager) 
        : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, 
               enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher, permissionsService, 
               settings)
    {
        this.AssignmentsRepository = assignmentsRepository;
        this.InterviewViewRepository = interviewViewRepository;
        this.dashboardViewModelFactory = dashboardViewModelFactory;
        this.vibrationService = vibrationService;
        this.backgroundServiceManager = backgroundServiceManager;
    }

    private GraphicsOverlayCollection graphicsOverlays = new GraphicsOverlayCollection();
    public GraphicsOverlayCollection GraphicsOverlays
    {
        get => this.graphicsOverlays;
        set => this.RaiseAndSetIfChanged(ref this.graphicsOverlays, value);
    }

    private bool isInProgress;
    public bool IsInProgress
    {
        get => this.isInProgress;
        set => this.RaiseAndSetIfChanged(ref this.isInProgress, value);
    }

    private bool showInterviews = true;
    public bool ShowInterviews
    {
        get => this.showInterviews;
        set => this.RaiseAndSetIfChanged(ref this.showInterviews, value);
    }
    private bool showAssignments = true;
    public bool ShowAssignments
    {
        get => this.showAssignments;
        set => this.RaiseAndSetIfChanged(ref this.showAssignments, value);
    }

    public override void Prepare(GeofencingViewModelArgs parameter)
    {
        var assignmentId = parameter.AssignmentId;

        assignment = AssignmentsRepository.GetById(assignmentId);
    }
        
    public override async Task Initialize()
    {
        await base.Initialize();
        
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

    private readonly GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
    
    protected override async Task AfterShapefileLoadedHandler()
    {
        await CheckMarkersAgainstShapefile();
    }

    protected override void ShowedFullMap()
    {
        base.ShowedFullMap();
    }

    protected async Task CheckMarkersAgainstShapefile()
    {
        IsWarningVisible = false;

        if (!ShapeFileLoaded 
            || graphicsOverlay.Graphics.Count <= 0 
            || LoadedShapefile?.SpatialReference == null) return;
        
        var queryParameters = new QueryParameters();

        //List<MapPoint> pointsToCheck = new List<MapPoint>();
        foreach (var graphic in graphicsOverlay.Graphics)
        {
            if (graphic.Geometry != null && graphic.Geometry.GeometryType == GeometryType.Point)
            { 
                var projectedPoint = graphic.Geometry.Project(LoadedShapefile.SpatialReference);
                if (projectedPoint is MapPoint mapPoint)
                {
                    //pointsToCheck.Add(mapPoint);
                    queryParameters.Geometry = mapPoint;
                    queryParameters.SpatialRelationship = SpatialRelationship.Intersects;
                    //queryParameters.ReturnGeometry = true;

                    var queryResult = await LoadedShapefile.QueryFeaturesAsync(queryParameters);
                    if (!queryResult.Any())
                    {
                        Warning = UIResources.AreaMap_ItemsOutsideDedicatedArea;
                        IsWarningVisible = true;
                        return;
                    }
                }
            }
        }
        
        /*Multipoint pointsMultipoint = new Multipoint(pointsToCheck, LoadedShapefile.SpatialReference);
        queryParameters.Geometry = pointsMultipoint;
        queryParameters.SpatialRelationship = SpatialRelationship.Intersects;
        queryParameters.ReturnGeometry = false;

        var queryResult = await LoadedShapefile.QueryFeaturesAsync(queryParameters);
        if (queryResult.Count() != pointsToCheck.Count())
        {
            Warning = UIResources.AreaMap_ItemsOutsideDedicatedArea;
            IsWarningVisible = true;
        }*/
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
    
    private MvxObservableCollection<MapDescription> availableMaps = new MvxObservableCollection<MapDescription>();
    
    public IMvxAsyncCommand NavigateToDashboardCommand => 
        new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToDashboardAsync());

    public IMvxCommand StartGeofencingCommand => 
        new MvxCommand(() =>
        {
            if (geofencingListener == null)
            {
                this.geofencingListener ??= new GeofencingListener(LoadedShapefile, vibrationService);
                this.backgroundServiceManager.StartListen(geofencingListener);
            }
            else
            {
                this.backgroundServiceManager.StopListen(geofencingListener);
                this.geofencingListener = null;
            }
        }, 
        () => LoadedShapefile != null);

    public override void Dispose()
    {
        base.Dispose();
    }
}

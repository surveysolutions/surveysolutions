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
using WB.Core.SharedKernels.Enumerator.Utils;
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



public class GeofencingViewModel: MarkersMapInteractionViewModel<GeofencingViewModelArgs>
{
    private AssignmentDocument assignment;
    private string locations = "";

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

    private readonly IVibrationService vibrationService;
    private readonly IGeolocationBackgroundServiceManager backgroundServiceManager;
    
    private GeofencingListener geofencingListener;
    private TestingListener testingListener;

    public GeofencingViewModel(IPrincipal principal, 
        IViewModelNavigationService viewModelNavigationService,
        IUserInteractionService userInteractionService,
        IMapService mapService,
        IEnumeratorSettings enumeratorSettings,
        ILogger logger,
        IMapUtilityService mapUtilityService,
        IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher,
        IPermissionsService permissionsService,
        IEnumeratorSettings settings,
        IVibrationService vibrationService,
        IGeolocationBackgroundServiceManager backgroundServiceManager,
        IDashboardViewModelFactory dashboardViewModelFactory,
        IAssignmentDocumentsStorage assignmentsRepository,
        IPlainStorage<InterviewView> interviewViewRepository) 
        : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, 
               enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher, permissionsService, 
               settings, dashboardViewModelFactory, assignmentsRepository, interviewViewRepository)
    {
        this.vibrationService = vibrationService;
        this.backgroundServiceManager = backgroundServiceManager;

        this.ShowInterviews = true;
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

    public override void Prepare(GeofencingViewModelArgs parameter)
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

    private async Task ToggleGeofencingService()
    {
        try
        {
            await permissionsService.AssureHasPermissionOrThrow<Permissions.LocationAlways>();

            if (geofencingListener == null)
            {
                this.geofencingListener ??= new GeofencingListener(LoadedShapefile, vibrationService);
                this.backgroundServiceManager.StartListen(geofencingListener);
                this.testingListener ??= new TestingListener(this);
                this.backgroundServiceManager.StartListen(testingListener);
                await SwitchLocator();
            }
            else
            {
                this.backgroundServiceManager.StopListen(testingListener);
                this.backgroundServiceManager.StopListen(geofencingListener);
                this.geofencingListener = null;
            }
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

    public class TestingListener : IGeolocationListener
    {
        private readonly GeofencingViewModel geofencingViewModel;

        public TestingListener(GeofencingViewModel geofencingViewModel)
        {
            this.geofencingViewModel = geofencingViewModel;
        }

        public Task OnGpsLocationChanged(GpsLocation location, INotificationManager notifications)
        {
            var loc = $"\r\n{location.Latitude}  {location.Longitude}";
            //geofencingViewModel.Locations = loc + geofencingViewModel.Locations;

            if (geofencingViewModel.geofencingListener.LastResult?.InShapefile ?? false)
            {
                geofencingViewModel.Locations = loc + " (OUT)";
            }
            else
            {
                geofencingViewModel.Locations = loc + " (In)";
            }
            
            return Task.CompletedTask;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}

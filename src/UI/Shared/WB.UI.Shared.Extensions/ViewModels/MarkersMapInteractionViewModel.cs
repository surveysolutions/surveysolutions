using System.Drawing;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Extensions.Services;

namespace WB.UI.Shared.Extensions.ViewModels;

public abstract class MarkersMapInteractionViewModel<TParam> : BaseMapInteractionViewModel<TParam>
{
    const string MarkerId = "marker_id";

    protected readonly GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

    private readonly IDashboardViewModelFactory dashboardViewModelFactory;
    protected readonly IAssignmentDocumentsStorage assignmentsRepository;
    protected readonly IPlainStorage<InterviewView> interviewViewRepository;

    protected List<AssignmentDocument> Assignments { get; private set; } = new List<AssignmentDocument>();
    protected List<InterviewView> Interviews  { get; private set; } = new List<InterviewView>();


    protected MarkersMapInteractionViewModel(IPrincipal principal, 
        IViewModelNavigationService viewModelNavigationService, 
        IMapService mapService, 
        IUserInteractionService userInteractionService, 
        ILogger logger, 
        IEnumeratorSettings enumeratorSettings, 
        IMapUtilityService mapUtilityService, 
        IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher, 
        IPermissionsService permissionsService, 
        IEnumeratorSettings settings,
        IDashboardViewModelFactory dashboardViewModelFactory,
        IAssignmentDocumentsStorage assignmentsRepository,
        IPlainStorage<InterviewView> interviewViewRepository
        ) 
        : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, enumeratorSettings,
            mapUtilityService, mainThreadAsyncDispatcher, permissionsService, settings)
    {
        this.dashboardViewModelFactory = dashboardViewModelFactory;
        this.assignmentsRepository = assignmentsRepository;
        this.interviewViewRepository = interviewViewRepository;
    }


    private MvxObservableCollection<IMarkerViewModel> availableMarkers = new MvxObservableCollection<IMarkerViewModel>();
    public MvxObservableCollection<IMarkerViewModel> AvailableMarkers
    {
        get => this.availableMarkers;
        set => this.RaiseAndSetIfChanged(ref this.availableMarkers, value);
    }

    private int? activeMarkerIndex;
    public int? ActiveMarkerIndex
    {
        get => this.activeMarkerIndex;
        set
        {
            if (activeMarkerIndex != value)
            {
                NavigateToMarkerByCard(value, activeMarkerIndex);
            }
            this.RaiseAndSetIfChanged(ref this.activeMarkerIndex, value);
        }
    }
    
    protected void NavigateToMarkerByCard(int? newPosition, int? oldPosition)
    {
        if (newPosition == oldPosition)
            return;

        if (oldPosition.HasValue && AvailableMarkers.Count > oldPosition.Value)
        {
            var marker = AvailableMarkers[oldPosition.Value];
            SetCommonMarkerStyle(marker);
        }

        if (newPosition.HasValue && AvailableMarkers.Count > newPosition.Value)
        {
            var marker = AvailableMarkers[newPosition.Value];
            SetFocusedMarkerStyle(marker);

            var projectedArea = GeometryEngine.Project(this.MapView.VisibleArea, SpatialReferences.Wgs84);
            var mapPoint = new MapPoint(marker.Longitude, marker.Latitude, SpatialReferences.Wgs84);
            if (projectedArea != null && !GeometryEngine.Contains(projectedArea, mapPoint))
                this.MapView.SetViewpointCenterAsync(marker.Latitude, marker.Longitude);
        }
    }
    
    void SetFocusedMarkerStyle(IMarkerViewModel marker) => SetMarkerStyle(marker, 100, 1.5);
    void SetCommonMarkerStyle(IMarkerViewModel marker) => SetMarkerStyle(marker, 0, 1);

    void SetMarkerStyle(IMarkerViewModel marker, int zIndex, double markerSize)
    {
        var graphic = graphicsOverlay.Graphics.FirstOrDefault(g => g.Attributes[MarkerId]?.ToString() == marker.Id);
        if (graphic != null)
        {
            graphic.ZIndex = zIndex;
            graphic.Symbol = (marker.Type == MarkerType.Assignment)
                ? GetAssignmentMarkerSymbol((IAssignmentMarkerViewModel)marker, markerSize)
                : GetInterviewMarkerSymbol((IInterviewMarkerViewModel)marker, markerSize);
        }
    }
    
    protected virtual CompositeSymbol GetAssignmentMarkerSymbol(IAssignmentMarkerViewModel assignment, double size = 1)
    {
        return new CompositeSymbol(new[]
        {
            new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.White, 15 * size), //for contrast
            new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.FromArgb(163, 113, 247), 11 * size)
        });
    }
    
    protected virtual Symbol GetInterviewMarkerSymbol(IInterviewMarkerViewModel interview, double size = 1)
    {
        Color markerColor;

        switch (interview.InterviewStatus)
        {
            case InterviewStatus.Created:
            case InterviewStatus.InterviewerAssigned:
            case InterviewStatus.Restarted:    
                markerColor = Color.FromArgb(24, 118, 207);
                break;
            case InterviewStatus.ApprovedBySupervisor:
                markerColor = Color.FromArgb(13,185,188);
                break;
            case InterviewStatus.Completed:
                markerColor = Color.FromArgb(54,141,54);
                break;
            case InterviewStatus.RejectedBySupervisor:
                markerColor = Color.FromArgb(227,74,21);
                break;
            case InterviewStatus.RejectedByHeadquarters:
                markerColor = Color.FromArgb(100,25,0);
                break;
            default:
                markerColor = Color.FromArgb(163, 113, 247);
                break;
        }

        return new CompositeSymbol(new[]
        {
            new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.White, 15 * size), //for contrast
            new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, markerColor, 11 * size)
        });
    }
    
    public IMvxCommand RefreshMarkersCommand => new MvxAsyncCommand(async() => await RefreshMarkers(setViewToMarkers: true));

    private readonly object graphicsOverlayLock = new object ();
    
    protected async Task RefreshMarkers(bool setViewToMarkers)
    {
        if (MapView?.Map?.SpatialReference != null)
        {
            await this.mainThreadAsyncDispatcher.ExecuteOnMainThreadAsync(() => { MapView.DismissCallout(); });

            try
            {
                lock (graphicsOverlayLock)
                {
                    graphicsOverlay.Graphics.Clear();

                    List<IMarkerViewModel> markers = new List<IMarkerViewModel>();
                    
                    if (ShowAssignments)
                    {
                        var filteredAssignments = FilteredAssignments();
                        var assignmentMarkers = filteredAssignments.Select(GetAssignmentMarkerViewModel).ToArray();
                        markers.AddRange(assignmentMarkers);
                        var assignmentsGraphics = GetAssignmentsMarkers(assignmentMarkers);
                        if (assignmentsGraphics.Count > 0)
                        {
                            graphicsOverlay.Graphics.AddRange(assignmentsGraphics);
                        }
                    }

                    if (ShowInterviews)
                    {
                        var filteredInterviews = FilteredInterviews();
                        var interviewMarkers = filteredInterviews.Select(GetInterviewMarkerViewModel).ToArray();
                        markers.AddRange(interviewMarkers);
                        var interviewsGraphics = GetInterviewsMarkers(interviewMarkers);
                        if (interviewsGraphics.Count > 0)
                        {
                            graphicsOverlay.Graphics.AddRange(interviewsGraphics);
                        }
                    }

                    if (markers.Count > 0)
                    {
                        double startLat = -90;
                        double startLng = 90;
                        foreach (var marker in markers)
                        {
                            if (startLat < marker.Latitude)
                                startLat = marker.Latitude;
                            if (startLng > marker.Longitude)
                                startLng = marker.Longitude;
                        }
                    
                        markers = markers
                            .OrderBy(m => GeometryHelper.GetDistance(startLat, startLng, m.Latitude, m.Longitude))
                            .ToList();
                    }

                    ActiveMarkerIndex = null;

                    this.AvailableMarkers.ToList().ForEach(uiItem =>
                    {
                        if (uiItem is InterviewDashboardItemViewModel interview)
                            interview.OnItemRemoved -= Markers_InterviewItemRemoved;
                        if (uiItem is IDashboardItemWithEvents withEvents)
                            withEvents.OnItemUpdated -= Markers_OnItemUpdated;
                        uiItem.DisposeIfDisposable();
                    });
                
                    AvailableMarkers.ReplaceWith(markers);

                    this.AvailableMarkers.ToList().ForEach(item =>
                    {
                        if (item is InterviewDashboardItemViewModel interview)
                            interview.OnItemRemoved += Markers_InterviewItemRemoved;
                        if (item is IDashboardItemWithEvents withEvents)
                            withEvents.OnItemUpdated += Markers_OnItemUpdated;
                    });
                }

                if (setViewToMarkers)
                    await SetViewToValues();
                
                await CheckMarkersAgainstShapefile();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
    
    protected void Markers_OnItemUpdated(object sender, EventArgs args)
    {
        IMarkerViewModel dashboardItem = sender as IMarkerViewModel;

        UpdateMarker(dashboardItem);
    }
    
    protected void UpdateMarker(IMarkerViewModel dashboardItem)
    {
        IMarkerViewModel newDashboardItem = null;
        
        if (dashboardItem is IAssignmentMarkerViewModel assignment)
        {
            var assignmentDocument = assignmentsRepository.GetById(assignment.AssignmentId);
            newDashboardItem = dashboardViewModelFactory.GetAssignment(assignmentDocument);
        }

        if (dashboardItem is IInterviewMarkerViewModel interview)
        {
            var interviewView = interviewViewRepository.GetById(interview.Id);
            newDashboardItem = dashboardViewModelFactory.GetInterview(interviewView);
        }
        
        if (newDashboardItem != null)
        {
            var indexOf = AvailableMarkers.IndexOf(dashboardItem);
            AvailableMarkers[indexOf] = newDashboardItem;
        }

        if (dashboardItem is IDashboardItemWithEvents dashboardItemWithEvents)
            dashboardItemWithEvents.OnItemUpdated -= Markers_OnItemUpdated;
        if (dashboardItem is InterviewDashboardItemViewModel oldInterview)
            oldInterview.OnItemRemoved -= Markers_InterviewItemRemoved;

        if (newDashboardItem is IDashboardItemWithEvents newDashboardItemWithEvents)
            newDashboardItemWithEvents.OnItemUpdated += Markers_OnItemUpdated;
        if (newDashboardItem is InterviewDashboardItemViewModel newInterview)
            newInterview.OnItemRemoved += Markers_InterviewItemRemoved;
        
        string markerId = dashboardItem.Id;
        var markerGraphic = graphicsOverlay.Graphics.FirstOrDefault(g => g.Attributes[MarkerId]?.ToString() == markerId);
        if (markerGraphic != null)
        {
            var indexOf = AvailableMarkers.IndexOf(newDashboardItem);
            var isActive = ActiveMarkerIndex == indexOf;
            if (isActive)
                SetFocusedMarkerStyle(newDashboardItem);
            else
                SetCommonMarkerStyle(newDashboardItem);
        }
    }

    protected async void Markers_InterviewItemRemoved(object sender, EventArgs e)
    {
        var item = (InterviewDashboardItemViewModel)sender;
        item.OnItemRemoved -= Markers_InterviewItemRemoved;
        item.OnItemUpdated -= Markers_OnItemUpdated;

        if (item.AssignmentId.HasValue)
        {
            assignmentsRepository.DecreaseInterviewsCount(item.AssignmentId.Value);

            this.AvailableMarkers
                .OfType<AssignmentDashboardItemViewModel>()
                .FirstOrDefault(x => x.AssignmentId == item.AssignmentId.Value)
                ?.DecreaseInterviewsCount();
        }

        ReloadEntities();
        await RefreshMarkers(false);
    }

    protected override async Task AfterShapefileLoadedHandler()
    {
        await CheckMarkersAgainstShapefile();
        ActiveMarkerIndex = null;
    }

    protected override void ShowedFullMap()
    {
        base.ShowedFullMap();
        ActiveMarkerIndex = null;
    }
    
    protected virtual async Task CheckMarkersAgainstShapefile()
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

    protected abstract List<InterviewView> FilteredInterviews();

    protected IInterviewMarkerViewModel GetInterviewMarkerViewModel(InterviewView interview)
    {
        return dashboardViewModelFactory.GetInterview(interview);
    }

    public virtual bool ShowInterviews { get; set; }
    public virtual bool ShowAssignments { get; set; }
    protected abstract List<AssignmentDocument> FilteredAssignments();
    
    private List<Graphic> GetAssignmentsMarkers(IEnumerable<IAssignmentMarkerViewModel> assignments)
    {
        var markersGraphic = new List<Graphic>();

        foreach (var assignment in assignments)
        {
            markersGraphic.Add(new Graphic(
                (MapPoint)GeometryEngine.Project(
                    new MapPoint(
                        assignment.Longitude,
                        assignment.Latitude,
                        SpatialReferences.Wgs84),
                    Map.SpatialReference),
                new[]
                {
                    new KeyValuePair<string, object>(MarkerId, assignment.Id),
                },
                GetAssignmentMarkerSymbol(assignment)));
        }

        return markersGraphic;
    }
    
    
    protected IAssignmentMarkerViewModel GetAssignmentMarkerViewModel(AssignmentDocument assignment)
    {
        return dashboardViewModelFactory.GetAssignment(assignment);
    }

    public async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
    {
        double tolerance = 10d; // Use larger tolerance for touch
        int maximumResults = 1; // Only return one graphic  
        bool onlyReturnPopups = false; // Don't only return popups

        try
        {
            IdentifyGraphicsOverlayResult identifyResults = await MapView.IdentifyGraphicsOverlayAsync(
                graphicsOverlay,
                e.Position,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            if (identifyResults.Graphics.Count > 0)
            {
                if (identifyResults.Graphics[0].Geometry is MapPoint projectedLocation)
                {
                    NavigateToCardByMarker(identifyResults, projectedLocation);
                }
            }
            else
            {
                ActiveMarkerIndex = null;
            }
        }
        catch (Exception ex)
        {
            this.logger.Error("Error on ", ex);
        }
    }

    protected void NavigateToCardByMarker(IdentifyGraphicsOverlayResult identifyResults,
        MapPoint projectedLocation)
    {
        var markerId = identifyResults.Graphics[0].Attributes[MarkerId].ToString();
        var markerViewModel = AvailableMarkers.FirstOrDefault(m => m.Id == markerId);
        if (markerViewModel != null)
        {
            var markerIndex = AvailableMarkers.IndexOf(markerViewModel);
            ActiveMarkerIndex = markerIndex < 0 ? null : markerIndex;
        }
    }
    
    private List<Graphic> GetInterviewsMarkers(IEnumerable<IInterviewMarkerViewModel> interviews)
    {
        var markersGraphics = new List<Graphic>();

        foreach (var interview in interviews)
        {
            markersGraphics.Add(new Graphic(
                (MapPoint)GeometryEngine.Project(
                    new MapPoint(
                        interview.Longitude,
                        interview.Latitude,
                        SpatialReferences.Wgs84),
                    Map.SpatialReference),
                new[]
                {
                    new KeyValuePair<string, object>(MarkerId, interview.Id),
                },
                GetInterviewMarkerSymbol(interview)));
        }

        return markersGraphics;
    }
    
    protected virtual void ReloadEntities()
    {
        Assignments = this.assignmentsRepository
            .LoadAll()
            .Where(x => x.LocationLatitude != null && (!x.Quantity.HasValue || (x.Quantity - (x.CreatedInterviewsCount ?? 0) > 0)))
            .ToList();

        Interviews = this.interviewViewRepository
            .Where(x => x.LocationLatitude != null).ToList();
    }
}

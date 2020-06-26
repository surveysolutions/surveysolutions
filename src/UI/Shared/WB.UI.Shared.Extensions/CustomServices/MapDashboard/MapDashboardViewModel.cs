using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using System.Drawing;
using Esri.ArcGISRuntime.Data;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using MapView = Esri.ArcGISRuntime.UI.Controls.MapView;

namespace WB.UI.Shared.Extensions.CustomServices.MapDashboard
{
    public class MapDashboardViewModel: BaseViewModel
    {
        private readonly ILogger logger;
        private readonly IUserInteractionService userInteractionService;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;

        public MapDashboardViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService,
            IMapService mapService,
            IFileSystemAccessor fileSystemAccessor,
            IAssignmentDocumentsStorage assignmentsRepository,
            IPlainStorage<InterviewView> interviewViewRepository,
            ILogger logger) : base(principal, viewModelNavigationService)
        {
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.mapService = mapService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.assignmentsRepository = assignmentsRepository;
            this.interviewViewRepository = interviewViewRepository;
        }

        private GraphicsOverlayCollection graphicsOverlays = new GraphicsOverlayCollection();
        public GraphicsOverlayCollection GraphicsOverlays
        {
            get => this.graphicsOverlays;
            set => this.RaiseAndSetIfChanged(ref this.graphicsOverlays, value);
        }

        private Map map;
        public Map Map
        {
            get => this.map;
            set => this.RaiseAndSetIfChanged(ref this.map, value);
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.RaiseAndSetIfChanged(ref this.isInProgress, value);
        }

        public MapView MapView { get; set; }

        public override async Task Initialize()
        {
            await base.Initialize();
            try
            {
                var localMaps = this.mapService.GetAvailableMaps(true);
                var defaultMap = this.mapService.PrepareAndGetDefaultMap();
                localMaps.Add(defaultMap);
                this.AvailableMaps = new MvxObservableCollection<MapDescription>(localMaps);
                this.SelectedMap = defaultMap.MapName;

                Basemap baseMap = await MapUtilityService.GetBaseMap(this.fileSystemAccessor, defaultMap).ConfigureAwait(false);
                this.Map = new Map(baseMap);

                this.GraphicsOverlays.Add(graphicsOverlay);
            }
            catch (Exception e)
            {
                logger.Error("Error on map initialization", e);
                throw;
            }
            
            this.Map.Loaded += async delegate (object sender, EventArgs e)
            {
                await UpdateBaseMap().ConfigureAwait(false);
                RefreshMarkers();
            };
        }

        private GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

        public IMvxCommand RefreshMarkersCommand =>
            new MvxCommand(RefreshMarkers);

        private void RefreshMarkers()
        {
            var interviewsMarkers = GetInterviewsMarkers();
            var assignmentsMarkers = GetAssignmentsMarkers();

            graphicsOverlay.Graphics.Clear();
            if (assignmentsMarkers.Count > 0 || interviewsMarkers.Count > 0)
            {
                graphicsOverlay.Graphics.AddRange(assignmentsMarkers);
                graphicsOverlay.Graphics.AddRange(interviewsMarkers);
            }
        }

        private List<Graphic> GetInterviewsMarkers()
        {
            var interviews = this.interviewViewRepository
                .Where(x => x.LocationLatitude != null).ToList();

            var markers = new List<Graphic>();

            foreach (var interview in interviews)
            {
                var questionnaireIdentity = QuestionnaireIdentity.Parse(interview.QuestionnaireId);
                var title = string.Format(EnumeratorUIResources.DashboardItem_Title, interview.QuestionnaireTitle,
                    questionnaireIdentity.Version);

                MapPoint point =
                    (MapPoint) GeometryEngine.Project(
                        new MapPoint(
                            interview.LocationLongitude.Value,
                            interview.LocationLatitude.Value,
                            SpatialReferences.Wgs84),
                        Map.SpatialReference);

                Color markerColor;

                switch (interview.Status)
                {
                    case InterviewStatus.Created:
                        markerColor = Color.Blue;
                        break;
                    case InterviewStatus.Completed:
                        markerColor = Color.Green;
                        break;
                    case InterviewStatus.RejectedBySupervisor:
                        markerColor = Color.Red;
                        break;
                    default:
                        markerColor = Color.White;
                        break;
                }

                markers.Add(new Graphic(
                    point,
                    new[]
                    {
                        new KeyValuePair<string, object>("id", ""),
                        new KeyValuePair<string, object>("interviewId", interview.Id),
                        new KeyValuePair<string, object>("interviewKey", interview.InterviewKey),
                        new KeyValuePair<string, object>("title", title),
                        new KeyValuePair<string, object>("sub_title", "")
                    },
                    new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, markerColor, 20)));
            }

            return markers;
        }

        private List<Graphic> GetAssignmentsMarkers()
        {
            var assignments = this.assignmentsRepository
                //.Query(x => x.LocationLatitude != null)
                .LoadAll()
                .Where(x => x.LocationLatitude != null)
                .ToList();

            var markers = new List<Graphic>();
            foreach (var assignment in assignments)
            {
                var questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);
                var title = string.Format(EnumeratorUIResources.DashboardItem_Title, assignment.Title,
                    questionnaireIdentity.Version);

                var interviewsByAssignmentCount = assignment.CreatedInterviewsCount ?? 0;
                var interviewsLeftByAssignmentCount = assignment.Quantity.GetValueOrDefault() - interviewsByAssignmentCount;

                string subTitle = "";

                if (assignment.Quantity.HasValue)
                {
                    if (interviewsLeftByAssignmentCount == 1)
                    {
                        subTitle = EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleSingleInterivew;
                    }
                    else
                    {
                        subTitle = string.Format(EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleCountdownFormat,
                            interviewsLeftByAssignmentCount, assignment.Quantity);
                    }
                }
                else
                {
                    subTitle = string.Format(EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleCountdown_UnlimitedFormat,
                        assignment.Quantity.GetValueOrDefault());
                }

                bool canCreateInterview =
                    !assignment.Quantity.HasValue || Math.Max(val1: 0, val2: interviewsLeftByAssignmentCount) > 0;

                MapPoint point =
                    (MapPoint) GeometryEngine.Project(
                        new MapPoint(
                            assignment.LocationLongitude.Value,
                            assignment.LocationLatitude.Value,
                            SpatialReferences.Wgs84),
                        Map.SpatialReference);

                Graphic pointGraphic = new Graphic(
                    point,
                    new[]
                    {
                        new KeyValuePair<string, object>("id", assignment.Id),
                        new KeyValuePair<string, object>("title", title),
                        new KeyValuePair<string, object>("sub_title", subTitle),
                        new KeyValuePair<string, object>("can_create", canCreateInterview)
                    },
                    new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Color.Gold, 20));

                markers.Add(pointGraphic);
            }

            return markers;
        }

        public async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            double tolerance = 10d; // Use larger tolerance for touch
            int maximumResults = 1; // Only return one graphic  
            bool onlyReturnPopups = false; // Don't only return popups

            try
            {
                // Use the following method to identify graphics in a specific graphics overlay
                IdentifyGraphicsOverlayResult identifyResults = await MapView.IdentifyGraphicsOverlayAsync(
                    graphicsOverlay,
                    e.Position,
                    tolerance,
                    onlyReturnPopups,
                    maximumResults);

                // Check if we got results
                if (identifyResults.Graphics.Count > 0)
                {
                    if (identifyResults.Graphics[0].Geometry is MapPoint projectedLocation)
                    {
                        string id = identifyResults.Graphics[0].Attributes["id"].ToString();
                        string title = identifyResults.Graphics[0].Attributes["title"] as string;
                        string subTitle = identifyResults.Graphics[0].Attributes["sub_title"] as string;

                        if (string.IsNullOrEmpty(id))
                        {
                            string interviewId = identifyResults.Graphics[0].Attributes["interviewId"].ToString();
                            string interviewKey = identifyResults.Graphics[0].Attributes["interviewKey"].ToString();

                            CalloutDefinition myCalloutDefinition =
                                new CalloutDefinition(interviewKey, $"{title}\r\n{subTitle}");
                            
                            myCalloutDefinition.ButtonImage =
                                await new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Green, 25)
                                    .CreateSwatchAsync(30, 30, 96, System.Drawing.Color.White);
                            myCalloutDefinition.OnButtonClick += OnInterviewButtonClick;
                            myCalloutDefinition.Tag = interviewId;
                            
                            MapView.ShowCalloutAt(projectedLocation, myCalloutDefinition);
                        }
                        else
                        {
                            bool canCreate = (bool)identifyResults.Graphics[0].Attributes["can_create"];

                            CalloutDefinition myCalloutDefinition =
                                new CalloutDefinition("#" + id, $"{title}\r\n{subTitle}");
                            if (canCreate)
                            {
                                myCalloutDefinition.ButtonImage =
                                    await new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.Blue, 25)
                                        .CreateSwatchAsync(30, 30, 96, System.Drawing.Color.White);
                                myCalloutDefinition.OnButtonClick += OnAssignmentButtonClick;
                                myCalloutDefinition.Tag = id;
                            }

                            MapView.ShowCalloutAt(projectedLocation, myCalloutDefinition);
                        }
                    }
                }
                else
                {
                    MapView.DismissCallout();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("Error on ", ex);
            }
        }

        private async void OnInterviewButtonClick(object calloutTag)
        {
            if (calloutTag is string tt)
            {
                await viewModelNavigationService.NavigateToInterviewAsync(tt, null);
            }
        }

        private void OnAssignmentButtonClick(object calloutTag)
        {
            if(calloutTag != null && (Int32.TryParse(calloutTag as string, out int assignmentId)))
            {
                //create interview from assignment
                viewModelNavigationService.NavigateToCreateAndLoadInterview(assignmentId);
            }
        }

        public IMvxAsyncCommand<MapDescription> SwitchMapCommand => new MvxAsyncCommand<MapDescription>(async (mapDescription) =>
        {
            this.SelectedMap = mapDescription.MapName;
            IsPanelVisible = false;

            await this.UpdateBaseMap();
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

        public async Task UpdateBaseMap()
        {
            var existingMap = this.AvailableMaps.FirstOrDefault(x => x.MapName == this.SelectedMap);

            if (existingMap != null)
            {
                var basemap = await MapUtilityService.GetBaseMap(this.fileSystemAccessor, existingMap);

                this.Map.Basemap = basemap;

                if (basemap?.BaseLayers[0]?.FullExtent != null)
                    await MapView.SetViewpointGeometryAsync(basemap.BaseLayers[0].FullExtent);
            }
        }

        private string selectedMap;
        public string SelectedMap
        {
            get => this.selectedMap;
            set => this.RaiseAndSetIfChanged(ref this.selectedMap, value);
        }

        public IMvxAsyncCommand RotateMapToNorth => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointRotationAsync(0));

        public IMvxAsyncCommand ZoomMapIn => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointScaleAsync(this.MapView.MapScale / 1.3));

        public IMvxAsyncCommand ZoomMapOut => new MvxAsyncCommand(async () =>
            await this.MapView?.SetViewpointScaleAsync(this.MapView.MapScale * 1.3));

        private bool isLocationServiceSwitchEnabled = true;
        private IMapService mapService;

        public bool IsLocationServiceSwitchEnabled
        {
            get => this.isLocationServiceSwitchEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isLocationServiceSwitchEnabled, value);
        }

        public IMvxAsyncCommand SwitchLocatorCommand => new MvxAsyncCommand(async () =>
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

                //try to stop service first to avoid crash
                await this.MapView.LocationDisplay.DataSource.StopAsync();

                await this.MapView.LocationDisplay.DataSource.StartAsync();
                this.MapView.LocationDisplay.IsEnabled = true;
                this.MapView.LocationDisplay.LocationChanged += LocationDisplayOnLocationChanged;
            }
            catch (Exception exc)
            {
                logger.Error("Error occurred on map location start.", exc);
                IsLocationServiceSwitchEnabled = false;
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

        private MvxObservableCollection<MapDescription> availableMaps = new MvxObservableCollection<MapDescription>();


        public MvxObservableCollection<MapDescription> AvailableMaps
        {
            get => this.availableMaps;
            protected set => this.RaiseAndSetIfChanged(ref this.availableMaps, value);
        }

        public IMvxCommand NavigateToDashboardCommand => 
            new MvxAsyncCommand(async () => await this.viewModelNavigationService.NavigateToDashboardAsync());
        
        public void Dispose()
        {
            if(this.MapView?.LocationDisplay!= null )
                this.MapView.LocationDisplay.LocationChanged -= LocationDisplayOnLocationChanged;

            if(MapView != null)
                MapView.GeoViewTapped -= OnMapViewTapped;
        }
    }
}

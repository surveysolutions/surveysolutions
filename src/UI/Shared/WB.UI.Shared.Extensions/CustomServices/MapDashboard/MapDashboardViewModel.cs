using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using MvvmCross.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
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
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher,
            ILogger logger) : base(principal, viewModelNavigationService)
        {
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.mapService = mapService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.assignmentsRepository = assignmentsRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.mainThreadDispatcher = mainThreadDispatcher;
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

        public MapView MapView { get; set; }

        private static string LastMap;

        public override async Task Initialize()
        {
            await base.Initialize();

            try
            {
                var localMaps = this.mapService.GetAvailableMaps(true);
                var defaultMap = this.mapService.PrepareAndGetDefaultMap();

                this.Map = new Map(await MapUtilityService.GetBaseMap(this.fileSystemAccessor, defaultMap).ConfigureAwait(false));

                MapDescription firstLocal = null;
                if (!string.IsNullOrEmpty(LastMap))
                {
                    firstLocal = localMaps.FirstOrDefault(x => x.MapName == LastMap);
                }

                firstLocal ??= localMaps.FirstOrDefault(x => x.MapType == MapType.LocalFile);
                
                var firstMap = firstLocal ?? defaultMap;

                localMaps.Add(defaultMap);
                this.AvailableMaps = new MvxObservableCollection<MapDescription>(localMaps);
                this.SelectedMap = firstMap.MapName;

                this.GraphicsOverlays.Add(graphicsOverlay);
            }
            catch (Exception e)
            {
                logger.Error("Error on map initialization", e);
                throw;
            }

            Assignments = this.assignmentsRepository
                .LoadAll()
                .Where(x => x.LocationLatitude != null)
                .ToList();

            Interviews = this.interviewViewRepository
                .Where(x => x.LocationLatitude != null).ToList();

            this.Map.Loaded += async delegate (object sender, EventArgs e)
            {
                CollectQuestionnaires();
                await UpdateBaseMap().ConfigureAwait(false);
                await RefreshMarkers();
            };

            PropertyChanged += OnPropertyChanged;
        }

        private void CollectQuestionnaires()
        {
            SelectedQuestionnaire = AllQuestionnaireDefault;

            List<QuestionnaireItem> result = new List<QuestionnaireItem>();

            if (ShowInterviews)
            {
                result.AddRange(
                    Interviews.Select(x =>new QuestionnaireItem(
                        x.QuestionnaireId,
                    string.Format(EnumeratorUIResources.DashboardItem_Title,
                        x.QuestionnaireTitle,
                        QuestionnaireIdentity.Parse(x.QuestionnaireId).Version)))
                );
            }

            if (ShowAssignments)
            {
                result.AddRange(
                    Assignments.Select(x => new QuestionnaireItem(
                        x.QuestionnaireId,
                            string.Format(EnumeratorUIResources.DashboardItem_Title,
                                x.Title,
                                QuestionnaireIdentity.Parse(x.QuestionnaireId).Version)))
                );
            }

            var questionnairesList = 
                result
                    .GroupBy(p => p.Title)
                    .Select(g => g.First())
                    .OrderBy(s => s.Title)
                    .ToList();

            questionnairesList.Add(AllQuestionnaireDefault);
            Questionnaires = new MvxObservableCollection<QuestionnaireItem>(questionnairesList);
        }

        private static readonly QuestionnaireItem AllQuestionnaireDefault = new QuestionnaireItem("", "All");

        public MvxObservableCollection<QuestionnaireItem> questionnaires = new MvxObservableCollection<QuestionnaireItem>();
        public MvxObservableCollection<QuestionnaireItem> Questionnaires
        {
            get => this.questionnaires;
            set => this.RaiseAndSetIfChanged(ref this.questionnaires, value);
        }

        private QuestionnaireItem selectedQuestionnaire = AllQuestionnaireDefault;
        public QuestionnaireItem SelectedQuestionnaire
        {
            get => this.selectedQuestionnaire;
            set => this.RaiseAndSetIfChanged(ref this.selectedQuestionnaire, value);
        }

        private MvxCommand<QuestionnaireItem> questionnaireSelectedCommand;
        public MvxCommand<QuestionnaireItem> QuestionnaireSelectedCommand => questionnaireSelectedCommand ??= new MvxCommand<QuestionnaireItem>(OnQuestionnaireSelectedCommand);

        private async void OnQuestionnaireSelectedCommand(QuestionnaireItem questionnaire)
        {
            await RefreshMarkers();
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShowInterviews) ||
                e.PropertyName == nameof(ShowAssignments))
            {
                this.CollectQuestionnaires();
                await this.RefreshMarkers();
            }
        }

        private readonly GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

        public IMvxCommand RefreshMarkersCommand => new MvxAsyncCommand(async() => await RefreshMarkers());

        private readonly object graphicsOverlayLock = new object ();
        private async Task RefreshMarkers()
        {
            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                MapView.DismissCallout();
            });

            Envelope graphicExtent = null;

            lock (graphicsOverlayLock)
            {
                graphicsOverlay.Graphics.Clear();

                if (ShowAssignments)
                {
                    var assignmentsMarkers = GetAssignmentsMarkers();
                    if (assignmentsMarkers.Count > 0)
                    {
                        graphicsOverlay.Graphics.AddRange(assignmentsMarkers);
                    }
                }

                if (ShowInterviews)
                {
                    var interviewsMarkers = GetInterviewsMarkers();
                    if (interviewsMarkers.Count > 0)
                    {
                        graphicsOverlay.Graphics.AddRange(interviewsMarkers);
                    }
                }

                if(graphicsOverlay.Graphics.Count > 0)
                    graphicExtent = GeometryEngine.CombineExtents(graphicsOverlay.Graphics.Select(graphic => graphic.Geometry));

                if (Map?.Basemap?.BaseLayers[0]?.FullExtent != null && graphicExtent != null)
                    graphicExtent = GeometryEngine.CombineExtents(Map.Basemap.BaseLayers[0].FullExtent, graphicExtent);
            }

            //MapView.Map.MinScale = 591657527.591555;
            //MapView.Map.MaxScale = 0;

            if (MapView.Map?.Basemap?.BaseLayers[0]?.FullExtent != null && graphicExtent != null)
                await MapView.SetViewpointAsync(
                    new Viewpoint(MapView.Map.Basemap.BaseLayers.First().FullExtent));

            /*if (graphicExtent != null)
                await MapView.SetViewpointAsync(new Viewpoint(graphicExtent), TimeSpan.FromSeconds(3));*/
        }

        private List<Graphic> GetInterviewsMarkers()
        {
            var markers = new List<Graphic>();

            var filteredInterviews =
                !string.IsNullOrEmpty(SelectedQuestionnaire?.QuestionnaireId)
                    ? Interviews.Where(x => x.QuestionnaireId == SelectedQuestionnaire.QuestionnaireId)
                        .ToList()
                    : Interviews;

            foreach (var interview in filteredInterviews)
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
                    case InterviewStatus.InterviewerAssigned:
                        markerColor = Color.Blue;
                        break;
                    case InterviewStatus.Completed:
                        markerColor = Color.Green;
                        break;
                    case InterviewStatus.RejectedBySupervisor:
                        markerColor = Color.Red;
                        break;
                    default:
                        markerColor = Color.Yellow;
                        break;
                }

                //for contrast
                markers.Add(new Graphic(point, new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.White, 22)));

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
                    new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, markerColor, 18)));
            }

            return markers;
        }


        private List<AssignmentDocument> Assignments = new List<AssignmentDocument>();
        private List<InterviewView> Interviews = new List<InterviewView>();

        private List<Graphic> GetAssignmentsMarkers()
        {
            var markers = new List<Graphic>();

            var filteredAssignments =
                !string.IsNullOrEmpty(SelectedQuestionnaire?.QuestionnaireId)  
                ? Assignments.Where(x => x.QuestionnaireId == SelectedQuestionnaire.QuestionnaireId)
                    .ToList()
                : Assignments;

            foreach (var assignment in filteredAssignments)
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

                //contrast
                markers.Add(new Graphic(point,
                    new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.White, 22)));

                markers.Add(new Graphic(
                    point,
                    new[]
                    {
                        new KeyValuePair<string, object>("id", assignment.Id),
                        new KeyValuePair<string, object>("title", title),
                        new KeyValuePair<string, object>("sub_title", subTitle),
                        new KeyValuePair<string, object>("can_create", canCreateInterview)
                    },
                    new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.Blue, 18)));
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
                                await new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Blue, 25)
                                    .CreateSwatchAsync(30, 30, 96, Color.FromArgb(246,250,251));
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
                                        .CreateSwatchAsync(30, 30, 96, Color.FromArgb(246, 250, 251));
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
            set
            {
                LastMap = value;
                this.RaiseAndSetIfChanged(ref this.selectedMap, value);
            }
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
        private IMvxMainThreadAsyncDispatcher mainThreadDispatcher;


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

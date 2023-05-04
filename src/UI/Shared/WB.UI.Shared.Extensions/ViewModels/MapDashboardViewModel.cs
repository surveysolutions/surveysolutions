using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
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
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.Extensions;
using WB.UI.Shared.Extensions.Services;
using Color = System.Drawing.Color;

namespace WB.UI.Shared.Extensions.ViewModels
{
    public abstract class MapDashboardViewModel: BaseMapInteractionViewModel<MapDashboardViewModelArgs>
    {
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        protected readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IDashboardViewModelFactory dashboardViewModelFactory;

        protected MapDashboardViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService,
            IMapService mapService,
            IAssignmentDocumentsStorage assignmentsRepository,
            IPlainStorage<InterviewView> interviewViewRepository,
            IEnumeratorSettings enumeratorSettings,
            ILogger logger,
            IMapUtilityService mapUtilityService,
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher,
            IDashboardViewModelFactory dashboardViewModelFactory) 
            : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, 
                   enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher)
        {
            this.assignmentsRepository = assignmentsRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.dashboardViewModelFactory = dashboardViewModelFactory;
            this.mainThreadDispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();
        }
        
        protected abstract InterviewStatus[] InterviewStatuses { get; }

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
        
        private bool showMarkersDetails = false;
        public bool ShowMarkersDetails
        {
            get => this.showMarkersDetails;
            set => this.RaiseAndSetIfChanged(ref this.showMarkersDetails, value);
        }


        public abstract bool SupportDifferentResponsible { get; }

        public override void Prepare(MapDashboardViewModelArgs parameter)
        {
        }

        protected void ReloadEntities()
        {
            Assignments = this.assignmentsRepository
                .LoadAll()
                .Where(x => x.LocationLatitude != null && (!x.Quantity.HasValue || (x.Quantity - (x.CreatedInterviewsCount ?? 0) > 0)))
                .ToList();

            Interviews = this.interviewViewRepository
                .Where(x => x.LocationLatitude != null).ToList();
        }

            
        public override async Task Initialize()
        {
            await base.Initialize();

            ReloadEntities();
            
            this.GraphicsOverlays.Add(graphicsOverlay);

            PropertyChanged += OnPropertyChanged;
        }

        public override async Task OnMapLoaded()
        {
            CollectQuestionnaires();
            CollectResponsibles();
            CollectInterviewStatuses();
            await RefreshMarkers();
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
        
        private void CollectQuestionnaires()
        {
            List<QuestionnaireItem> result = new List<QuestionnaireItem>();

            if (ShowInterviews)
            {
                result.AddRange(Interviews.Select(ToQuestionnaireItem));
            }

            if (ShowAssignments)
            {
                result.AddRange(Assignments.Select(ToQuestionnaireItem));
            }

            var questionnairesList = new List<QuestionnaireItem> {AllQuestionnaireDefault};

            questionnairesList.AddRange( 
                result
                    .GroupBy(p => p.Title)
                    .Select(g => g.First())
                    .OrderBy(s => s.Title)
                    .ToList());

            Questionnaires = new MvxObservableCollection<QuestionnaireItem>(questionnairesList);

            if (SelectedQuestionnaire != AllQuestionnaireDefault)
                SelectedQuestionnaire = AllQuestionnaireDefault;
        }
        
        protected virtual void CollectResponsibles()
        {
        }

        private void CollectInterviewStatuses()
        {
            var statusItems = new List<StatusItem> { AllStatusDefault };

            InterviewStatuses.ForEach(s => statusItems.Add(new StatusItem(s, s.ToLocalizeString())));

            Statuses = new MvxObservableCollection<StatusItem>(statusItems);

            if (SelectedStatus != AllStatusDefault)
                SelectedStatus = AllStatusDefault;
        }

        private QuestionnaireItem ToQuestionnaireItem(AssignmentDocument assignmentDocument)
        {
            return new QuestionnaireItem(
                QuestionnaireIdentity.Parse(assignmentDocument.QuestionnaireId).QuestionnaireId.FormatGuid(),
                assignmentDocument.Title);
        }

        private QuestionnaireItem ToQuestionnaireItem(InterviewView interviewView)
        {
            return new QuestionnaireItem(
                QuestionnaireIdentity.Parse(interviewView.QuestionnaireId).QuestionnaireId.FormatGuid(),
                interviewView.QuestionnaireTitle);
        }

        private static readonly QuestionnaireItem AllQuestionnaireDefault = new QuestionnaireItem("", UIResources.MapDashboard_AllQuestionnaires);

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
        public MvxCommand<QuestionnaireItem> QuestionnaireSelectedCommand => 
            questionnaireSelectedCommand ??= new MvxCommand<QuestionnaireItem>(OnQuestionnaireSelectedCommand);

        private async void OnQuestionnaireSelectedCommand(QuestionnaireItem questionnaire)
        {
            if (SelectedQuestionnaire.Title == questionnaire.Title && 
                SelectedQuestionnaire.QuestionnaireId == questionnaire.QuestionnaireId)
                return;
            
            SelectedQuestionnaire = questionnaire;
            await RefreshMarkers();
        }

        protected static readonly ResponsibleItem AllResponsibleDefault = new ResponsibleItem(null, UIResources.MapDashboard_AllResponsibles);

        private MvxObservableCollection<ResponsibleItem> responsibles = new MvxObservableCollection<ResponsibleItem>();
        public MvxObservableCollection<ResponsibleItem> Responsibles
        {
            get => this.responsibles;
            set => this.RaiseAndSetIfChanged(ref this.responsibles, value);
        }

        private ResponsibleItem selectedResponsible = AllResponsibleDefault;
        public ResponsibleItem SelectedResponsible
        {
            get => this.selectedResponsible;
            set => this.RaiseAndSetIfChanged(ref this.selectedResponsible, value);
        }

        private MvxCommand<ResponsibleItem> responsibleSelectedCommand;
        public MvxCommand<ResponsibleItem> ResponsibleSelectedCommand => 
            responsibleSelectedCommand ??= new MvxCommand<ResponsibleItem>(OnResponsibleSelectedCommand);

        private async void OnResponsibleSelectedCommand(ResponsibleItem responsible)
        {
            if (SelectedResponsible.Title == responsible.Title && 
                SelectedResponsible.ResponsibleId == responsible.ResponsibleId)
                return;
            
            SelectedResponsible = responsible;
            await RefreshMarkers();
        }

        private static readonly StatusItem AllStatusDefault = new StatusItem(null, UIResources.MapDashboard_AllStatuses);

        private MvxObservableCollection<StatusItem> statuses = new MvxObservableCollection<StatusItem>();
        public MvxObservableCollection<StatusItem> Statuses
        {
            get => this.statuses;
            set => this.RaiseAndSetIfChanged(ref this.statuses, value);
        }

        private StatusItem selectedStatus = AllStatusDefault;
        public StatusItem SelectedStatus
        {
            get => this.selectedStatus;
            set => this.RaiseAndSetIfChanged(ref this.selectedStatus, value);
        }

        private MvxCommand<StatusItem> statusSelectedCommand;
        public MvxCommand<StatusItem> StatusSelectedCommand => 
            statusSelectedCommand ??= new MvxCommand<StatusItem>(OnStatusSelectedCommand);

        private async void OnStatusSelectedCommand(StatusItem status)
        {
            if (SelectedStatus.Title == status.Title && 
                SelectedStatus.Status == status.Status)
                return;
            
            SelectedStatus = status;
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

        protected async Task RefreshMarkers()
        {
            if (MapView?.Map?.SpatialReference != null)
            {
                await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() => { MapView.DismissCallout(); });

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

                        AvailableMarkers.ReplaceWith(markers);
                    }

                    //MapView.Map.MinScale = 591657527.591555;
                    //MapView.Map.MaxScale = 0;
                    await SetViewToValues();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
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
                        new KeyValuePair<string, object>("marker_id", interview.Id),
                    },
                    GetInterviewMarkerSymbol(interview)));
            }

            return markersGraphics;
        }

        private List<InterviewView> FilteredInterviews()
        {
            var filteredInterviews = Interviews;

            if (!string.IsNullOrEmpty(SelectedQuestionnaire?.QuestionnaireId))
                filteredInterviews = filteredInterviews
                    .Where(x => x.QuestionnaireId.StartsWith(SelectedQuestionnaire.QuestionnaireId))
                    .ToList();

            if (SelectedResponsible?.ResponsibleId.HasValue ?? false)
                filteredInterviews = filteredInterviews
                    .Where(x => x.ResponsibleId == SelectedResponsible.ResponsibleId)
                    .ToList();

            if (SelectedStatus?.Status != null)
                filteredInterviews = filteredInterviews
                    .Where(x => x.Status == SelectedStatus.Status)
                    .ToList();
            return filteredInterviews;
        }

        protected IInterviewMarkerViewModel GetInterviewMarkerViewModel(InterviewView interview)
        {
            return dashboardViewModelFactory.GetInterview(interview);
        }

        protected abstract Symbol GetInterviewMarkerSymbol(IInterviewMarkerViewModel interview, int size = 1);

        private List<AssignmentDocument> Assignments = new List<AssignmentDocument>();
        private List<InterviewView> Interviews = new List<InterviewView>();

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
                        new KeyValuePair<string, object>("marker_id", assignment.Id),
                    },
                    GetAssignmentMarkerSymbol(assignment)));
            }

            return markersGraphic;
        }

        private List<AssignmentDocument> FilteredAssignments()
        {
            var filteredAssignments = Assignments;

            if (!string.IsNullOrEmpty(SelectedQuestionnaire?.QuestionnaireId))
                filteredAssignments = filteredAssignments
                    .Where(x => x.QuestionnaireId.StartsWith(SelectedQuestionnaire.QuestionnaireId))
                    .ToList();

            if (SelectedResponsible?.ResponsibleId.HasValue ?? false)
                filteredAssignments = filteredAssignments
                    .Where(x => x.ResponsibleId == SelectedResponsible.ResponsibleId)
                    .ToList();
            return filteredAssignments;
        }

        protected virtual CompositeSymbol GetAssignmentMarkerSymbol(IAssignmentMarkerViewModel assignment, int size = 1)
        {
            return new CompositeSymbol(new[]
            {
                new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.White, 22 * size), //for contrast
                new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.FromArgb(0x2a,0x81,0xcb), 16 * size)
            });
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
                        ShowMarkersDetails = true;
                        NavigateToCardByMarker(identifyResults, projectedLocation);
                    }
                }
                else
                {
                    ShowMarkersDetails = false;
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
            var markerId = identifyResults.Graphics[0].Attributes["marker_id"].ToString();
            var markerViewModel = AvailableMarkers.FirstOrDefault(m => m.Id == markerId);
            if (markerViewModel != null)
            {
                var markerIndex = AvailableMarkers.IndexOf(markerViewModel);
                ActiveMarkerIndex = markerIndex < 0 ? null : markerIndex;
            }
        }

        protected void NavigateToMarkerByCard(int? newPosition, int? oldPosition)
        {
            if (!newPosition.HasValue || newPosition == oldPosition)
                return;

            var newMarker = AvailableMarkers[newPosition.Value];
            var newGraphic = graphicsOverlay.Graphics.FirstOrDefault(g => g.Attributes["marker_id"]?.ToString() == newMarker.Id);
            if (newGraphic != null)
            {
                newGraphic.ZIndex = 100;
                newGraphic.Symbol = (newMarker.Type == MarkerType.Assignment)
                    ? GetAssignmentMarkerSymbol((IAssignmentMarkerViewModel)newMarker, 3)
                    : GetInterviewMarkerSymbol((IInterviewMarkerViewModel)newMarker, 3);
            }

            var oldMarker = AvailableMarkers[newPosition.Value];
            var oldGraphic = graphicsOverlay.Graphics.FirstOrDefault(g => g.Attributes["marker_id"]?.ToString() == oldMarker.Id);
            if (oldGraphic != null)
            {
                oldGraphic.ZIndex = 0;
                oldGraphic.Symbol = (oldMarker.Type == MarkerType.Assignment)
                    ? GetAssignmentMarkerSymbol((IAssignmentMarkerViewModel)oldMarker, 1)
                    : GetInterviewMarkerSymbol((IInterviewMarkerViewModel)oldMarker, 1);
            }

            this.MapView.SetViewpointCenterAsync(newMarker.Latitude, newMarker.Longitude);
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
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        
        public IMvxAsyncCommand NavigateToDashboardCommand => 
            new MvxAsyncCommand(async () => await this.ViewModelNavigationService.NavigateToDashboardAsync());
        
        public override void Dispose()
        {
            if (MapView != null)
                MapView.GeoViewTapped -= OnMapViewTapped;

            base.Dispose();
        }
    }

    public class MapDashboardViewModelArgs
    {
    }
}

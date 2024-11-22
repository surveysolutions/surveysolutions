using System.ComponentModel;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
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
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.Extensions;
using WB.UI.Shared.Extensions.Services;
using Color = System.Drawing.Color;

namespace WB.UI.Shared.Extensions.ViewModels
{
    [InterviewEntryPoint]
    public abstract class MapDashboardViewModel: MarkersMapInteractionViewModel<MapDashboardViewModelArgs>
    {
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
            IDashboardViewModelFactory dashboardViewModelFactory,
            IPermissionsService permissionsService,
            IEnumeratorSettings settings) 
            : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, 
                   enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher, permissionsService, 
                   settings, dashboardViewModelFactory, assignmentsRepository, interviewViewRepository)
        {
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
        public override bool ShowInterviews
        {
            get => this.showInterviews;
            set => this.RaiseAndSetIfChanged(ref this.showInterviews, value);
        }
        private bool showAssignments = true;
        public override bool ShowAssignments
        {
            get => this.showAssignments;
            set => this.RaiseAndSetIfChanged(ref this.showAssignments, value);
        }

        
        
        public abstract bool SupportDifferentResponsible { get; }

        public override void Prepare(MapDashboardViewModelArgs parameter)
        {
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
            await RefreshMarkers(setViewToMarkers: true);
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
            await RefreshMarkers(setViewToMarkers: true);
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
            await RefreshMarkers(setViewToMarkers: true);
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShowInterviews) ||
                e.PropertyName == nameof(ShowAssignments))
            {
                this.CollectQuestionnaires();
                await this.RefreshMarkers(setViewToMarkers: true);
            }
        }
        

        protected override void ShowedFullMap()
        {
            base.ShowedFullMap();
            ActiveMarkerIndex = null;
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
                ActiveMarkerIndex = null;
                await MapView.SetViewpointAsync(new Viewpoint(graphicExtent), TimeSpan.FromSeconds(4));
            }
        }



        protected override List<InterviewView> FilteredInterviews()
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


        protected override List<AssignmentDocument> FilteredAssignments()
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
        
        public override void Dispose()
        {
            if (MapView != null)
                MapView.GeoViewTapped -= OnMapViewTapped;

            this.AvailableMarkers.ToList().ForEach(item =>
            {
                if (item is IDashboardItemWithEvents withEvents)
                    withEvents.OnItemUpdated -= Markers_OnItemUpdated;
                if (item is InterviewDashboardItemViewModel interview)
                    interview.OnItemRemoved -= Markers_InterviewItemRemoved;

                item?.DisposeIfDisposable();
            });
            
            base.Dispose();
        }
    }

    public class MapDashboardViewModelArgs
    {
    }
}

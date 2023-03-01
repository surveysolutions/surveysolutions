using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
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
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.Services;

namespace WB.UI.Shared.Extensions.ViewModels
{
    public class MapDashboardViewModel: BaseMapInteractionViewModel<MapDashboardViewModelArgs>
    {
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<InterviewerDocument> usersRepository;

        private static readonly InterviewStatus[] InterviewerStatuses =
        {
            InterviewStatus.Created,
            InterviewStatus.InterviewerAssigned,
            InterviewStatus.Restarted,
            InterviewStatus.RejectedBySupervisor,
            InterviewStatus.Completed,
        };

        private static readonly InterviewStatus[] SupervisorStatuses =
            InterviewerStatuses.Concat(new []
            {
                InterviewStatus.SupervisorAssigned,
                InterviewStatus.RejectedByHeadquarters,
            }).ToArray();
        
        public MapDashboardViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService,
            IMapService mapService,
            IAssignmentDocumentsStorage assignmentsRepository,
            IPlainStorage<InterviewView> interviewViewRepository,
            IEnumeratorSettings enumeratorSettings,
            ILogger logger,
            IMapUtilityService mapUtilityService,
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher,
            IPlainStorage<InterviewerDocument> usersRepository) 
            : base(principal, viewModelNavigationService, mapService, userInteractionService, logger, 
                   enumeratorSettings, mapUtilityService, mainThreadAsyncDispatcher)
        {
            this.assignmentsRepository = assignmentsRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.usersRepository = usersRepository;
            this.mainThreadDispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();
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
        
        public bool SupportDifferentResponsible { get; set; }
        
        public override void Prepare(MapDashboardViewModelArgs parameter)
        {
            SupportDifferentResponsible = parameter.SupportDifferentResponsible;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Assignments = this.assignmentsRepository
                .LoadAll()
                .Where(x => x.LocationLatitude != null && (!x.Quantity.HasValue || (x.Quantity - (x.CreatedInterviewsCount ?? 0) > 0)))
                .ToList();

            Interviews = this.interviewViewRepository
                .Where(x => x.LocationLatitude != null).ToList();

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
        
        private void CollectResponsibles()
        {
            if (!SupportDifferentResponsible)
                return;
            
            List<ResponsibleItem> result = usersRepository.LoadAll()
                .Where(x => !x.IsLockedByHeadquarters && !x.IsLockedBySupervisor)
                .Select(user => new ResponsibleItem(user.InterviewerId, user.UserName))
                .OrderBy(x => x.Title)
                .ToList();

            var responsibleItems = new List<ResponsibleItem>
            {
                AllResponsibleDefault,
                new ResponsibleItem(Principal.CurrentUserIdentity.UserId, Principal.CurrentUserIdentity.Name),
            };
            responsibleItems.AddRange(result);

            Responsibles = new MvxObservableCollection<ResponsibleItem>(responsibleItems);

            if (SelectedResponsible != AllResponsibleDefault)
                SelectedResponsible = AllResponsibleDefault;
        }

        private void CollectInterviewStatuses()
        {
            var statusItems = new List<StatusItem> { AllStatusDefault };

            var statuses = SupportDifferentResponsible
                ? SupervisorStatuses
                : InterviewerStatuses;
            statuses.ForEach(s => statusItems.Add(new StatusItem(s, s.ToString())));

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
        
        private static readonly ResponsibleItem AllResponsibleDefault = new ResponsibleItem(null, UIResources.MapDashboard_AllResponsibles);

        public MvxObservableCollection<ResponsibleItem> responsibles = new MvxObservableCollection<ResponsibleItem>();
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

        public MvxObservableCollection<StatusItem> statuses = new MvxObservableCollection<StatusItem>();
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

        private async Task RefreshMarkers()
        {
            if (MapView?.Map?.SpatialReference != null)
            {
                await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() => { MapView.DismissCallout(); });

                try
                {
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

        private List<Graphic> GetInterviewsMarkers()
        {
            var markers = new List<Graphic>();

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

            foreach (var interview in filteredInterviews)
            {
                var questionnaireIdentity = QuestionnaireIdentity.Parse(interview.QuestionnaireId);
                var title = string.Format(EnumeratorUIResources.DashboardItem_Title, interview.QuestionnaireTitle,
                    questionnaireIdentity.Version);

                Color markerColor;

                switch (interview.Status)
                {
                    case InterviewStatus.Created:
                    case InterviewStatus.InterviewerAssigned:
                    case InterviewStatus.Restarted:    
                        markerColor = Color.FromArgb(0x2a, 0x81, 0xcb);
                        break;
                    case InterviewStatus.Completed:
                        markerColor = Color.FromArgb(0x1f,0x95,0x00);
                        break;
                    case InterviewStatus.RejectedBySupervisor:
                        markerColor = Color.FromArgb(0xe4,0x51,0x2b);
                        break;
                    default:
                        markerColor = Color.Yellow;
                        break;
                }

                var responsibleName = Responsibles.FirstOrDefault(r => interview.ResponsibleId == r.ResponsibleId)?.Title;

                markers.Add(new Graphic(
                    (MapPoint)GeometryEngine.Project(
                        new MapPoint(
                            interview.LocationLongitude.Value,
                            interview.LocationLatitude.Value,
                            SpatialReferences.Wgs84),
                        Map.SpatialReference),
                    new[]
                    {
                        new KeyValuePair<string, object>("id", ""),
                        new KeyValuePair<string, object>("responsible", responsibleName),
                        new KeyValuePair<string, object>("interviewId", interview.Id),
                        new KeyValuePair<string, object>("interviewKey", interview.InterviewKey),
                        new KeyValuePair<string, object>("title", title),
                        new KeyValuePair<string, object>("sub_title", "")
                    },
                    new CompositeSymbol(new[]
                    {
                        new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.White, 22), //for contrast
                        new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, markerColor, 16)
                    })));
            }

            return markers;
        }


        private List<AssignmentDocument> Assignments = new List<AssignmentDocument>();
        private List<InterviewView> Interviews = new List<InterviewView>();

        private List<Graphic> GetAssignmentsMarkers()
        {
            var markers = new List<Graphic>();

            var filteredAssignments = Assignments;
            
            if (!string.IsNullOrEmpty(SelectedQuestionnaire?.QuestionnaireId)) 
                 filteredAssignments = filteredAssignments
                         .Where(x => x.QuestionnaireId.StartsWith(SelectedQuestionnaire.QuestionnaireId))
                         .ToList();
            
            if (SelectedResponsible?.ResponsibleId.HasValue ?? false) 
                 filteredAssignments = filteredAssignments
                         .Where(x => x.ResponsibleId == SelectedResponsible.ResponsibleId)
                         .ToList();

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

                markers.Add(new Graphic(
                    (MapPoint)GeometryEngine.Project(
                        new MapPoint(
                            assignment.LocationLongitude.Value,
                            assignment.LocationLatitude.Value,
                            SpatialReferences.Wgs84),
                        Map.SpatialReference),
                    new[]
                    {
                        new KeyValuePair<string, object>("id", assignment.Id),
                        new KeyValuePair<string, object>("responsible", assignment.ResponsibleName),
                        new KeyValuePair<string, object>("title", title),
                        new KeyValuePair<string, object>("sub_title", subTitle),
                        new KeyValuePair<string, object>("can_create", canCreateInterview)
                    },
                    new CompositeSymbol(new[]
                    {
                        new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.White, 22), //for contrast
                        new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.FromArgb(0x2a,0x81,0xcb), 16)
                    })));
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
                        string responsible = identifyResults.Graphics[0].Attributes["responsible"] as string;

                        var popupTemplate = SupportDifferentResponsible
                            ? $"{title}\r\n{responsible}\r\n{subTitle}"
                            : $"{title}\r\n{subTitle}";
                        
                        if (string.IsNullOrEmpty(id))
                        {
                            string interviewId = identifyResults.Graphics[0].Attributes["interviewId"].ToString();
                            string interviewKey = identifyResults.Graphics[0].Attributes["interviewKey"].ToString();

                            CalloutDefinition myCalloutDefinition =
                                new CalloutDefinition(interviewKey, popupTemplate)
                                {
                                    ButtonImage = await new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle,
                                            Color.Blue, 25).CreateSwatchAsync(96)
                                };

                            myCalloutDefinition.OnButtonClick += OnInterviewButtonClick;
                            myCalloutDefinition.Tag = interviewId;
                            
                            MapView.ShowCalloutAt(projectedLocation, myCalloutDefinition);
                        }
                        else
                        {
                            var assignmentInfo = identifyResults.Graphics[0].Attributes;
                            bool canCreate = (bool)assignmentInfo["can_create"];

                            CalloutDefinition myCalloutDefinition =
                                new CalloutDefinition("#" + id, popupTemplate);
                            if (canCreate)
                            {
                                myCalloutDefinition.ButtonImage =
                                    await new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.Blue, 25)
                                        .CreateSwatchAsync(96);
                                myCalloutDefinition.OnButtonClick += tag => OnAssignmentButtonClick(assignmentInfo, tag);
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
            if (calloutTag is string interviewId)
            {
                var interview = interviewViewRepository.GetById(interviewId);
                if (interview != null && interview.Status == InterviewStatus.Completed)
                {
                    var isReopen = await UserInteractionService.ConfirmAsync(
                        EnumeratorUIResources.Dashboard_Reinitialize_Interview_Message,
                        okButton: UIResources.Yes,
                        cancelButton: UIResources.No);

                    if (!isReopen)
                    {
                        return;
                    }
                }

                await ViewModelNavigationService.NavigateToInterviewAsync(interviewId, null);
            }
        }

        private void OnAssignmentButtonClick(IDictionary<string, object> assignmentInfo, object calloutTag)
        {
            bool isCreating = assignmentInfo.ContainsKey("creating");
            if (isCreating)
                return;
            
            assignmentInfo["creating"] = true;
            if(calloutTag != null && (Int32.TryParse(calloutTag as string, out int assignmentId)))
            {
                //create interview from assignment
                ViewModelNavigationService.NavigateToCreateAndLoadInterview(assignmentId);
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
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        
        public IMvxCommand NavigateToDashboardCommand => 
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
        public bool SupportDifferentResponsible { get; set; } = false;
    }
}

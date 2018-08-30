using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class BaseInterviewViewModel : SingleInterviewViewModel, IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        protected readonly IStatefulInterviewRepository interviewRepository;
        public NavigationState navigationState { get; }
        private readonly AnswerNotifier answerNotifier;
        private readonly GroupStateViewModel groupState;
        private readonly InterviewStateViewModel interviewState;
        private readonly CoverStateViewModel coverState;
        protected readonly IViewModelNavigationService viewModelNavigationService;
        protected readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IEnumeratorSettings enumeratorSettings;
        public static BaseInterviewViewModel CurrentInterviewScope;

        protected BaseInterviewViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            GroupStateViewModel groupState,
            InterviewStateViewModel interviewState,
            CoverStateViewModel coverState,
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IInterviewViewModelFactory interviewViewModelFactory,
            ICommandService commandService,
            VibrationViewModel vibrationViewModel,
            IEnumeratorSettings enumeratorSettings)
            : base(principal, viewModelNavigationService, commandService, vibrationViewModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.navigationState = navigationState ?? throw new ArgumentNullException(nameof(navigationState));
            this.answerNotifier = answerNotifier;
            this.groupState = groupState;
            this.interviewState = interviewState;
            this.coverState = coverState;
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.enumeratorSettings = enumeratorSettings;

            this.BreadCrumbs = breadCrumbsViewModel;
            this.Sections = sectionsViewModel;
            CurrentInterviewScope = this;
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.RaiseAndSetIfChanged(ref this.isInProgress, value); }
        }

        public abstract Task NavigateBack();

        private NavigationIdentity targetNavigationIdentity;

        public override void Prepare(InterviewViewModelArgs parameter)
        {
            base.Prepare(parameter);
            this.targetNavigationIdentity = parameter.NavigationIdentity;
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            var interview = this.interviewRepository.Get(InterviewId);

            if (interview == null)
            {
                await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId).ConfigureAwait(false);
                return;
            }

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            if (questionnaire == null)
                throw new QuestionnaireException("Questionnaire not found")
                {
                    Data = {{"QuestionnaireId", interview.QuestionnaireId}}
                };

            this.HasNotEmptyNoteFromSupervior = !string.IsNullOrWhiteSpace(interview.GetLastSupervisorComment());
            this.HasCommentsFromSupervior = interview.GetCommentedBySupervisorQuestionsVisibledToInterviewer().Any();

            var prefilledQuestions = questionnaire
                .GetPrefilledQuestions();

            this.HasPrefilledQuestions = prefilledQuestions
                .Any(questionId => questionnaire.GetQuestionType(questionId) != QuestionType.GpsCoordinates);
            this.HasEdiablePrefilledQuestions = prefilledQuestions
                .All(questionId => interview.GetQuestion(new Identity(questionId, null))?.IsReadonly ?? true);

            this.QuestionnaireTitle = questionnaire.Title;

            assignmentId = interview.GetAssignmentId();

            interviewKey = interview.GetInterviewKey();
            this.InterviewKey = interviewKey == null ? null : String.Format(UIResources.InterviewKey, interviewKey.ToString());

            this.availableLanguages = questionnaire.GetTranslationLanguages();
            this.currentLanguage = interview.Language;

            this.BreadCrumbs.Init(InterviewId, this.navigationState);
            this.Sections.Init(InterviewId, this.navigationState);

            this.navigationState.Init(interviewId: InterviewId, questionnaireId: interview.QuestionnaireId);
            this.navigationState.ScreenChanged += this.OnScreenChanged;

            await this.navigationState.NavigateTo(this.targetNavigationIdentity ?? this.GetDefaultScreenToNavigate(questionnaire)).ConfigureAwait(false);

            this.answerNotifier.QuestionAnswered += this.AnswerNotifierOnQuestionAnswered;

            this.IsVariablesShowed = this.enumeratorSettings.ShowVariables;
            this.IsSuccessfullyLoaded = true;
        }

        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            if (this.targetNavigationIdentity != null)
            {
                bundle.Data["lastNavigationIdentity"] = JsonConvert.SerializeObject(this.targetNavigationIdentity);
            }
        }

        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            if (state.Data.ContainsKey("lastNavigationIdentity"))
            {
                var serialized = state.Data["lastNavigationIdentity"];
                this.targetNavigationIdentity = JsonConvert.DeserializeObject<NavigationIdentity>(serialized);
            }
        }

        protected virtual NavigationIdentity GetDefaultScreenToNavigate(IQuestionnaire questionnaire)
        {
            return NavigationIdentity.CreateForGroup(new Identity(questionnaire.GetAllSections().First(), RosterVector.Empty));
        }

        private void AnswerNotifierOnQuestionAnswered(object sender, EventArgs eventArgs)
        {
            if (this.navigationState.CurrentScreenType == ScreenType.Group)
            {
                this.UpdateGroupStatus(this.navigationState.CurrentGroup);
            }
        }

        private void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            switch (eventArgs.TargetStage)
            {
                case ScreenType.Complete:
                    this.vibrationViewModel.Disable();
                    this.interviewState.Init(this.navigationState.InterviewId, null);
                    this.Status = this.interviewState.Status;
                    break;
                case ScreenType.Cover:
                    this.vibrationViewModel.Disable();
                    this.coverState.Init(this.navigationState.InterviewId, null);
                    this.Status = this.coverState.Status;
                    break;
                case ScreenType.Group:
                    this.vibrationViewModel.Enable();
                    var interview = this.interviewRepository.Get(this.InterviewId);
                    IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(eventArgs.TargetGroup);
                    this.answerNotifier.Init(this.InterviewId, questionsToListen.ToArray());
                    this.UpdateGroupStatus(eventArgs.TargetGroup);
                    break;
            }

            this.targetNavigationIdentity = new NavigationIdentity
            {
                AnchoredElementIdentity = eventArgs.AnchoredElementIdentity,
                TargetGroup = eventArgs.TargetGroup,
                TargetScreen = eventArgs.TargetStage
            };

            this.CurrentStage.DisposeIfDisposable();
            this.CurrentStage = this.GetInterviewStageViewModel(eventArgs);
            this.RaisePropertyChanged(() => this.CurrentStage);
        }

        private InterviewStageViewModel GetInterviewStageViewModel(ScreenChangedEventArgs eventArgs)
            => new InterviewStageViewModel(
                this.UpdateCurrentScreenViewModel(eventArgs),
                this.GetNavigationDirection(eventArgs));

        private NavigationDirection GetNavigationDirection(ScreenChangedEventArgs eventArgs)
        {
            switch (eventArgs.TargetStage)
            {
                case ScreenType.Cover: return NavigationDirection.Previous;
                case ScreenType.Complete: return NavigationDirection.Next;
                case ScreenType.Identifying:
                    switch (eventArgs.PreviousStage)
                    {
                        case ScreenType.Cover: return NavigationDirection.Next;
                        default: return NavigationDirection.Previous;
                    }

                default:
                    switch (eventArgs.PreviousStage)
                    {
                        case ScreenType.Cover: return NavigationDirection.Next;
                        case ScreenType.Complete: return NavigationDirection.Previous;
                        case ScreenType.Identifying: return NavigationDirection.Next;

                        default:
                            var interview = this.interviewRepository.Get(this.InterviewId);

                            if (eventArgs.PreviousGroup == null || eventArgs.TargetGroup == null)
                                return NavigationDirection.Next;

                            var isTargetGroupInsidePrevious = eventArgs.TargetGroup.UnwrapReferences(interview.GetParentGroup).Contains(eventArgs.PreviousGroup);
                            if (isTargetGroupInsidePrevious)
                                return NavigationDirection.Inside;

                            var isPreviousGroupInsideTarget = eventArgs.PreviousGroup.UnwrapReferences(interview.GetParentGroup).Contains(eventArgs.TargetGroup);
                            if (isPreviousGroupInsideTarget)
                                return NavigationDirection.Outside;

                            return interview.IsFirstEntityBeforeSecond(eventArgs.PreviousGroup, eventArgs.TargetGroup)
                                ? NavigationDirection.Next
                                : NavigationDirection.Previous;
                    }
            }
        }

        protected virtual MvxViewModel UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs)
        {
            switch (this.navigationState.CurrentScreenType)
            {
                case ScreenType.Complete:
                    var completeInterviewViewModel = this.interviewViewModelFactory.GetNew<CompleteInterviewViewModel>();
                    completeInterviewViewModel.Configure(this.InterviewId, this.navigationState);
                    return completeInterviewViewModel;
                case ScreenType.Cover:
                    var coverInterviewViewModel = this.interviewViewModelFactory.GetNew<CoverInterviewViewModel>();
                    coverInterviewViewModel.Configure(this.InterviewId, this.navigationState);
                    return coverInterviewViewModel;
                case ScreenType.Group:
                    var activeStageViewModel = this.interviewViewModelFactory.GetNew<EnumerationStageViewModel>();
                    activeStageViewModel.Configure(this.InterviewId, this.navigationState, eventArgs.TargetGroup, eventArgs.AnchoredElementIdentity);
                    return activeStageViewModel;
                case ScreenType.Overview:
                    var overviewViewModel = this.interviewViewModelFactory.GetNew<OverviewViewModel>();
                    overviewViewModel.Configure(this.InterviewId);
                    return overviewViewModel;
                default:
                    return null;
            }
        }

        private void UpdateGroupStatus(Identity groupIdentity)
        {
            this.groupState.Init(this.navigationState.InterviewId, groupIdentity);
            this.Status = this.groupState.Status;
        }

        private GroupStatus status;
        public GroupStatus Status
        {
            get { return this.status; }
            private set
            {
                if (this.status != value)
                {
                    this.status = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public BreadCrumbsViewModel BreadCrumbs { get; set; }
        public SideBarSectionsViewModel Sections { get; set; }
        public string QuestionnaireTitle { get; set; }
        public string InterviewKey { get; set; }
        protected InterviewKey interviewKey;
        protected int? assignmentId;

        public bool HasPrefilledQuestions { get; set; }
        public bool HasEdiablePrefilledQuestions { get; set; }

        public bool HasCommentsFromSupervior { get; set; }
        public bool HasNotEmptyNoteFromSupervior { get; set; }

        public InterviewStageViewModel CurrentStage { get; private set; }

        public string Title { get; private set; }

        public bool IsVariablesShowed { get; private set; }

        private string currentLanguage;
        public override string CurrentLanguage => this.currentLanguage;

        private IReadOnlyCollection<string> availableLanguages;
        public override IReadOnlyCollection<string> AvailableLanguages => this.availableLanguages;

        public IMvxCommand NavigateToDashboardCommand => new MvxAsyncCommand(async () =>
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
        });

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.viewModelNavigationService.SignOutAndNavigateToLoginAsync);
        public IMvxCommand NavigateToSettingsCommand => new MvxCommand(this.viewModelNavigationService.NavigateToSettings);
        public IMvxCommand NavigateToDiagnosticsPageCommand => new MvxAsyncCommand(this.viewModelNavigationService.NavigateToAsync<DiagnosticsViewModel>);

        public void NavigateToPreviousViewModel(Action navigateToIfHistoryIsEmpty)
            => this.navigationState.NavigateBack(navigateToIfHistoryIsEmpty);

        public override void Dispose()
        {
            base.Dispose();

            this.navigationState.ScreenChanged -= this.OnScreenChanged;
            this.answerNotifier.QuestionAnswered -= this.AnswerNotifierOnQuestionAnswered;
            this.CurrentStage.DisposeIfDisposable();
            this.answerNotifier.Dispose();
            this.BreadCrumbs.Dispose();
            this.Sections.Dispose();
        }
    }
}

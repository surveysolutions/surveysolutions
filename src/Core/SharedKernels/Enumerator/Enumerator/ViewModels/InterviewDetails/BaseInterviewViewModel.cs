using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public enum NavigationDirection
    {
        Inside = 1,
        Outside = 2,
        Next = 3,
        Previous = 4,
    }

    public class InterviewStageViewModel : MvxViewModel, IDisposable
    {
        public InterviewStageViewModel(MvxViewModel stage, NavigationDirection direction)
        {
            this.Stage = stage;
            this.Direction = direction;
        }

        public MvxViewModel Stage { get; }
        public NavigationDirection Direction { get; }
        public void Dispose() => this.Stage.DisposeIfDisposable();
    }

    public abstract class BaseInterviewViewModel : SingleInterviewViewModel, IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly NavigationState navigationState;
        private readonly AnswerNotifier answerNotifier;
        private readonly GroupStateViewModel groupState;
        private readonly InterviewStateViewModel interviewState;
        private readonly CoverStateViewModel coverState;
        private readonly IViewModelNavigationService viewModelNavigationService;
        protected readonly IInterviewViewModelFactory interviewViewModelFactory;
        private IStatefulInterview interview;
        private readonly IJsonAllTypesSerializer jsonSerializer;

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
            IJsonAllTypesSerializer jsonSerializer)
            : base(principal, viewModelNavigationService, commandService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.navigationState = navigationState;
            this.answerNotifier = answerNotifier;
            this.groupState = groupState;
            this.interviewState = interviewState;
            this.coverState = coverState;
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.jsonSerializer = jsonSerializer;

            this.BreadCrumbs = breadCrumbsViewModel;
            this.Sections = sectionsViewModel;
        }

        public abstract void NavigateBack();

        private NavigationIdentity targetNavigationIdentity;

        public void Init(string interviewId, string jsonNavigationIdentity)
        {
            base.Initialize(interviewId);

            if (jsonNavigationIdentity != null)
            {
                this.targetNavigationIdentity = this.jsonSerializer.Deserialize<NavigationIdentity>(jsonNavigationIdentity);
            }
        }

        public override void Load()
        {
            if (this.interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            interview = this.interviewRepository.Get(interviewId);

            if (interview == null)
            {
                this.viewModelNavigationService.NavigateToDashboard();
                return;
            }

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity, this.interview.Language);
            if (questionnaire == null)
                throw new Exception("Questionnaire not found. QuestionnaireId: " + interview.QuestionnaireId);

            this.HasNotEmptyNoteFromSupervior = !string.IsNullOrWhiteSpace(this.interview.GetLastSupervisorComment());
            this.HasCommentsFromSupervior = this.interview.CountCommentedQuestions() > 0;
            this.HasPrefilledQuestions = questionnaire
                .GetPrefilledQuestions()
                .Any(questionId => questionnaire.GetQuestionType(questionId) != QuestionType.GpsCoordinates);

            this.QuestionnaireTitle = questionnaire.Title;
        
            this.availableLanguages = questionnaire.GetTranslationLanguages();
            this.currentLanguage = this.interview.Language;

            this.BreadCrumbs.Init(interviewId, this.navigationState);
            this.Sections.Init(interviewId, interview.QuestionnaireIdentity, this.navigationState);

            this.navigationState.Init(interviewId: interviewId, questionnaireId: interview.QuestionnaireId);
            this.navigationState.ScreenChanged += this.OnScreenChanged;
                
            this.navigationState.NavigateTo(this.targetNavigationIdentity ?? this.GetDefaultScreenToNavigate(questionnaire));

            this.answerNotifier.QuestionAnswered += this.AnswerNotifierOnQuestionAnswered;

            this.IsSuccessfullyLoaded = true;
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
                    this.interviewState.Init(this.navigationState.InterviewId, null);
                    this.Status = this.interviewState.Status;
                    break;
                case ScreenType.Cover:
                    this.coverState.Init(this.navigationState.InterviewId, null);
                    this.Status = this.coverState.Status;
                    break;
                default:
                    IEnumerable<Identity> questionsToListen = this.interview.GetChildQuestions(eventArgs.TargetGroup);
                    this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());
                    this.UpdateGroupStatus(eventArgs.TargetGroup);
                    break;
            }

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

                default:
                    switch (eventArgs.PreviousStage)
                    {
                        case ScreenType.Cover: return NavigationDirection.Next;
                        case ScreenType.Complete: return NavigationDirection.Previous;

                        default:
                            return NavigationDirection.Inside;
                    }
            }
        }

        protected abstract MvxViewModel UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs);

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

        public bool HasPrefilledQuestions { get; set; }
        public bool HasCommentsFromSupervior { get; set; }
        public bool HasNotEmptyNoteFromSupervior { get; set; }

        public InterviewStageViewModel CurrentStage { get; private set; }
        public string Title { get; private set; }

        private string currentLanguage;
        public override string CurrentLanguage => this.currentLanguage;

        private IReadOnlyCollection<string> availableLanguages;
        public override IReadOnlyCollection<string> AvailableLanguages => this.availableLanguages;

        public void NavigateToPreviousViewModel(Action navigateToIfHistoryIsEmpty)
            => this.navigationState.NavigateBack(navigateToIfHistoryIsEmpty);

        public void Dispose()
        {
            this.navigationState.ScreenChanged -= this.OnScreenChanged;
            this.answerNotifier.QuestionAnswered -= this.AnswerNotifierOnQuestionAnswered;
            this.CurrentStage.DisposeIfDisposable();
            this.answerNotifier.Dispose();
            this.BreadCrumbs.Dispose();
            this.Sections.Dispose();
        }
    }
}
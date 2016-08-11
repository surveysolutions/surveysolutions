using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using MvvmCross.Core.ViewModels;
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
    public abstract class BaseInterviewViewModel : SingleInterviewViewModel, IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly NavigationState navigationState;
        private readonly AnswerNotifier answerNotifier;
        private readonly IAnswerToStringService answerToStringService;
        private readonly GroupStateViewModel groupState;
        private readonly InterviewStateViewModel interviewState;
        private readonly IViewModelNavigationService viewModelNavigationService;
        protected readonly IInterviewViewModelFactory interviewViewModelFactory;
        private IStatefulInterview interview;
        private readonly IJsonAllTypesSerializer jsonSerializer;

        protected BaseInterviewViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel, 
            BreadCrumbsViewModel breadCrumbsViewModel,
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            GroupStateViewModel groupState, 
            InterviewStateViewModel interviewState,
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
            this.answerToStringService = answerToStringService;
            this.groupState = groupState;
            this.interviewState = interviewState;
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

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire
                .GetPrefilledQuestions()
                .Select(questionId => new SideBarPrefillQuestion
                {
                    Question = questionnaire.GetQuestionTitle(questionId),
                    Answer = this.GetAnswer(questionnaire, questionId),
                    StatsInvisible = questionnaire.GetQuestionType(questionId) == QuestionType.GpsCoordinates,
                })
                .ToList();

            this.availableLanguages = questionnaire.GetTranslationLanguages();
            this.currentLanguage = this.interview.Language;

            this.BreadCrumbs.Init(interviewId, this.navigationState);
            this.Sections.Init(interviewId, interview.QuestionnaireIdentity, this.navigationState);

            this.navigationState.Init(interviewId: interviewId, questionnaireId: interview.QuestionnaireId);
            this.navigationState.ScreenChanged += this.OnScreenChanged;
                
            this.navigationState.NavigateTo(this.targetNavigationIdentity ?? GetDefaultScreenToNaviage(questionnaire));

            this.answerNotifier.QuestionAnswered += this.AnswerNotifierOnQuestionAnswered;

            this.IsSuccessfullyLoaded = true;
        }

        protected virtual NavigationIdentity GetDefaultScreenToNaviage(IQuestionnaire questionnaire)
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
            if (eventArgs.TargetScreen != ScreenType.Group)
            {
                this.UpdateInterviewStatus(null);
            }
            else
            {
                IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(eventArgs.TargetGroup);

                this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());

                this.UpdateGroupStatus(eventArgs.TargetGroup);
            }

            this.CurrentStage.DisposeIfDisposable();
            this.CurrentStage = this.UpdateCurrentScreenViewModel(eventArgs);
            this.RaisePropertyChanged(() => this.CurrentStage);
        }

        protected abstract MvxViewModel UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs);

        private void UpdateGroupStatus(Identity groupIdentity)
        {
            this.groupState.Init(this.navigationState.InterviewId, groupIdentity);
            this.Status = this.groupState.Status;
        }

        private void UpdateInterviewStatus(Identity groupIdentity)
        {
            this.interviewState.Init(this.navigationState.InterviewId, groupIdentity);
            this.Status = this.interviewState.Status;
        }

        private string GetAnswer(IQuestionnaire questionnaire, Guid referenceToQuestionId)
        {
            var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(referenceToQuestionId, new decimal[0]);
            var interviewAnswer = interview.Answers.ContainsKey(identityAsString) ? interview.Answers[identityAsString] : null;
            return this.answerToStringService.AnswerToUIString(referenceToQuestionId, interviewAnswer, interview, questionnaire);
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
        public IEnumerable<SideBarPrefillQuestion> PrefilledQuestions { get; set; }

        public MvxViewModel CurrentStage { get; private set; }
        public string Title { get; private set; }

        private string currentLanguage;
        public override string CurrentLanguage => this.currentLanguage;

        private IReadOnlyCollection<string> availableLanguages;
        public override IReadOnlyCollection<string> AvailableLanguages => this.availableLanguages;

        public IEnumerable<SideBarPrefillQuestion> PrefilledQuestionsStats => this.PrefilledQuestions.Where(x => !x.StatsInvisible);

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
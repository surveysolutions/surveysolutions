using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class EnumeratorInterviewViewModel : BaseViewModel, IDisposable
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly NavigationState navigationState;
        private readonly AnswerNotifier answerNotifier;
        private readonly IAnswerToStringService answerToStringService;
        private readonly GroupStateViewModel groupState;
        private readonly InterviewStateViewModel interviewState;
        protected string interviewId;

        protected EnumeratorInterviewViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel, 
            BreadCrumbsViewModel breadCrumbsViewModel,
            ActiveStageViewModel stageViewModel, 
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            GroupStateViewModel groupState, 
            InterviewStateViewModel interviewState)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.interviewRepository = interviewRepository;
            this.navigationState = navigationState;
            this.answerNotifier = answerNotifier;
            this.answerToStringService = answerToStringService;
            this.groupState = groupState;
            this.interviewState = interviewState;

            this.BreadCrumbs = breadCrumbsViewModel;
            this.CurrentStage = stageViewModel;
            this.Sections = sectionsViewModel;
        }

        public async Task Init(string interviewId)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.interviewId = interviewId;
            var interview = this.interviewRepository.Get(interviewId);
            if (interview == null) throw new Exception("Interview is null.");
            var questionnaireModel = this.questionnaireModelRepository.GetById(interview.QuestionnaireId);
            if (questionnaireModel == null) throw new Exception("questionnaire is null");
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);
            if (questionnaire == null) throw new Exception("questionnaire is null. QuestionnaireId: " + interview.QuestionnaireId);

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaireModel.PrefilledQuestionsIds
                .Select(referenceToQuestion => new SideBarPrefillQuestion
                {
                    Question = questionnaireModel.Questions[referenceToQuestion.Id].Title,
                    Answer = this.GetAnswer(interview, questionnaireModel, referenceToQuestion),
                    StatsInvisible = referenceToQuestion.ModelType == typeof(GpsCoordinatesQuestionModel)
                })
                .ToList();

            this.BreadCrumbs.Init(interviewId, this.navigationState);
            this.Sections.Init(interview.QuestionnaireId, interviewId, this.navigationState);
            this.CurrentStage.Init(interviewId, this.navigationState);

            this.navigationState.Init(interviewId: interviewId, questionnaireId: interview.QuestionnaireId);
            this.navigationState.ScreenChanged += this.OnScreenChanged;
            await this.navigationState.NavigateToAsync(NavigationIdentity.CreateForGroup(new Identity(questionnaireModel.GroupsWithFirstLevelChildrenAsReferences.Keys.First(), new decimal[0])));

            this.answerNotifier.QuestionAnswered += this.AnswerNotifierOnQuestionAnswered;
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
                this.UpdateInterviewStatus(null, ScreenType.Complete);
                return;
            }

            IStatefulInterview interview = this.interviewRepository.Get(this.navigationState.InterviewId);
            IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(eventArgs.TargetGroup);

            this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());

            this.UpdateGroupStatus(eventArgs.TargetGroup);
        }

        private void UpdateGroupStatus(Identity groupIdentity, ScreenType type = ScreenType.Group)
        {
            this.groupState.Init(this.navigationState.InterviewId, groupIdentity);
            this.Status = this.groupState.Status;
        }

        private void UpdateInterviewStatus(Identity groupIdentity, ScreenType type = ScreenType.Group)
        {
            this.interviewState.Init(this.navigationState.InterviewId, groupIdentity);
            this.Status = this.interviewState.Status;
        }

        private string GetAnswer(IStatefulInterview interview, QuestionnaireModel questionnaire, QuestionnaireReferenceModel referenceToQuestion)
        {
            var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(referenceToQuestion.Id, new decimal[0]);
            var interviewAnswer = interview.Answers.ContainsKey(identityAsString) ? interview.Answers[identityAsString] : null;
            var questionModel = questionnaire.Questions[referenceToQuestion.Id];
            return this.answerToStringService.AnswerToUIString(questionModel, interviewAnswer, interview, questionnaire);
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
        public ActiveStageViewModel CurrentStage { get; set; }
        public SideBarSectionsViewModel Sections { get; set; }
        public string QuestionnaireTitle { get; set; }
        public IEnumerable<dynamic> PrefilledQuestions { get; set; }

        public IEnumerable<dynamic> PrefilledQuestionsStats
        {
            get { return PrefilledQuestions.Where(x => !x.StatsInvisible); }

        }

        public void Dispose()
        {
            this.navigationState.ScreenChanged -= this.OnScreenChanged;
            this.answerNotifier.QuestionAnswered -= this.AnswerNotifierOnQuestionAnswered;
            this.CurrentStage.Dispose();
            this.answerNotifier.Dispose();
            this.BreadCrumbs.Dispose();
            this.Sections.Dispose();
        }
    }
}
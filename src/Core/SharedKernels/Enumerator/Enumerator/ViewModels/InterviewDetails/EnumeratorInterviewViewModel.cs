using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class EnumeratorInterviewViewModel : BaseViewModel
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        protected readonly NavigationState navigationState;
        private readonly AnswerNotifier answerNotifier;
        private readonly IAnswerToStringService answerToStringService;
        private readonly GroupStateViewModel groupState;
        protected string interviewId;

        public EnumeratorInterviewViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            SideBarSectionsViewModel sectionsViewModel, 
            BreadCrumbsViewModel breadCrumbsViewModel,
            ActiveGroupViewModel groupViewModel, 
            NavigationState navigationState,
            AnswerNotifier answerNotifier,
            GroupStateViewModel groupState)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.navigationState = navigationState;
            this.answerNotifier = answerNotifier;
            this.answerToStringService = answerToStringService;
            this.groupState = groupState;

            this.BreadCrumbs = breadCrumbsViewModel;
            this.CurrentGroup = groupViewModel;
            this.Sections = sectionsViewModel;
        }

        public async Task Init(string interviewId)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            this.interviewId = interviewId;
            var interview = this.interviewRepository.Get(interviewId);
            if (interview == null) throw new Exception("Interview is null.");
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            if (questionnaire == null) throw new Exception("questionnaire is null");

            this.QuestionnaireTitle = questionnaire.Title;
            this.PrefilledQuestions = questionnaire.PrefilledQuestionsIds
                .Select(referenceToQuestion => new SideBarPrefillQuestion
                {
                    Question = questionnaire.Questions[referenceToQuestion.Id].Title,
                    Answer = GetAnswer(interview, questionnaire, referenceToQuestion)
                })
                .ToList();

            this.BreadCrumbs.Init(interviewId, this.navigationState);
            this.Sections.Init(interview.QuestionnaireId, interviewId, this.navigationState);
            this.CurrentGroup.Init(interviewId, this.navigationState);

            this.navigationState.Init(interviewId: interviewId, questionnaireId: interview.QuestionnaireId);
            this.navigationState.GroupChanged += NavigationStateOnOnGroupChanged;
            await this.navigationState.NavigateToAsync(groupIdentity: new Identity(questionnaire.GroupsWithFirstLevelChildrenAsReferences.Keys.First(), new decimal[0]));

            this.answerNotifier.QuestionAnswered += AnswerNotifierOnQuestionAnswered;
        }

        private void AnswerNotifierOnQuestionAnswered(object sender, EventArgs eventArgs)
        {
            this.UpdateInterviewStatus(navigationState.CurrentGroup);
        }

        private void NavigationStateOnOnGroupChanged(GroupChangedEventArgs newGroupIdentity)
        {
            var interview = this.interviewRepository.Get(navigationState.InterviewId);
            IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(newGroupIdentity.TargetGroup);

            this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());

            this.UpdateInterviewStatus(newGroupIdentity.TargetGroup);
        }

        private void UpdateInterviewStatus(Identity groupIdentity)
        {
            this.groupState.Init(this.navigationState.InterviewId, groupIdentity);

            this.Status = this.groupState.Status;
        }

        private string GetAnswer(IStatefulInterview interview, QuestionnaireModel questionnaire, QuestionnaireReferenceModel referenceToQuestion)
        {
            var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(referenceToQuestion.Id, new decimal[0]);
            var interviewAnswer = interview.Answers.ContainsKey(identityAsString) ? interview.Answers[identityAsString] : null;
            var questionModel = questionnaire.Questions[referenceToQuestion.Id];
            return this.answerToStringService.AnswerToUIString(questionModel, interviewAnswer);
        }

        private GroupStatus status;

        public GroupStatus Status
        {
            get { return this.status; }
            private set
            {
                if (status != value)
                {
                    this.status = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public BreadCrumbsViewModel BreadCrumbs { get; set; }
        public ActiveGroupViewModel CurrentGroup { get; set; }
        public SideBarSectionsViewModel Sections { get; set; }
        public string QuestionnaireTitle { get; set; }
        public IEnumerable<dynamic> PrefilledQuestions { get; set; }


        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}
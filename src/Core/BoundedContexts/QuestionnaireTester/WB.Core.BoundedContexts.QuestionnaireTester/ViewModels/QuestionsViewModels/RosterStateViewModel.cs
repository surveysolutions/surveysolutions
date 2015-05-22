using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RosterStateViewModel : MvxNotifyPropertyChanged
    {
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        public EnablementViewModel Enablement { get; private set; }
        private NavigationState navigationState;
        private Identity rosterIdentity;

        public string QuestionnaireRosterTitle { get; private set; }
        public string InterviewRosterTitle { get; private set; }
        public int AnsweredQuestionsCount { get; private set; }
        public int SubgroupsCount { get; private set; }
        public int QuestionsCount { get; private set; }
        public int InvalidAnswersCount { get; private set; }
        public bool IsStarted { get { return AnsweredQuestionsCount > 0; } }
        public GroupStatus Status { get; private set; }

        private IMvxCommand navigateToRosterCommand;
        public IMvxCommand NavigateToRosterCommand
        {
            get { return navigateToRosterCommand ?? (navigateToRosterCommand = new MvxCommand(NavigateToRoster)); }
        }

        public RosterStateViewModel(
            IStatefullInterviewRepository interviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            EnablementViewModel enablement)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.Enablement = enablement;
        }

        public void Init(string interviewId, Identity rosterIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.navigationState = navigationState;
            this.rosterIdentity = rosterIdentity;

            this.Enablement.Init(interviewId, this.rosterIdentity, navigationState);

            this.QuestionnaireRosterTitle = questionnaire.GroupsWithoutNestedChildren[this.rosterIdentity.Id].Title;

            var roster = (InterviewRoster)interview.Groups[ConversionHelper.ConvertIdAndRosterVectorToString(rosterIdentity.Id, rosterIdentity.RosterVector)];

            this.InterviewRosterTitle = roster.Title;

            var groupEntities = questionnaire.GroupsWithoutNestedChildren[this.rosterIdentity.Id].Children;
            var groupQuestions = groupEntities.Where(x => x.ModelType == typeof(BaseQuestionModel));

            this.QuestionsCount = groupQuestions.Count();
            this.SubgroupsCount = groupEntities.Count(x => x.ModelType == typeof(GroupModel) || x.ModelType == typeof(RosterModel));
            this.AnsweredQuestionsCount = interview.Answers.Values.Count(x => groupQuestions.Any(y => y.Id == x.Id) &&
                                                                              x.RosterVector.SequenceEqual(this.rosterIdentity.RosterVector) && x.IsAnswered);

            this.InvalidAnswersCount = 0;

            this.InitStatus();
        }

        private void InitStatus()
        {
            if (this.InvalidAnswersCount > 0)
                this.Status = GroupStatus.HasInvlidAnswers;

            if (this.AnsweredQuestionsCount > 0)
                this.Status = GroupStatus.Started;

            if (this.QuestionsCount == this.AnsweredQuestionsCount)
                this.Status = GroupStatus.Completed;

            this.Status = GroupStatus.NotStarted;
        }

        private void NavigateToRoster()
        {
            this.navigationState.NavigateTo(this.rosterIdentity);
        }
    }
}
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
    public class GroupViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;

        private NavigationState navigationState;
        private Identity groupIdentity;

        public EnablementViewModel Enablement { get; private set; }
        public string Title { get; private set; }
        public int AnsweredQuestionsCount { get; private set; }
        public int SubgroupsCount { get; private set; }
        public int QuestionsCount { get; private set; }
        public int InvalidAnswersCount { get; private set; }
        public GroupStatus Status { get; private set; }

        public bool IsStarted
        {
            get { return this.Status == GroupStatus.Started; }
        }

        private IMvxCommand navigateToGroupCommand;

        public IMvxCommand NavigateToGroupCommand
        {
            get { return navigateToGroupCommand ?? (navigateToGroupCommand = new MvxCommand(NavigateToGroup)); }
        }

        public GroupViewModel(
            IStatefullInterviewRepository interviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            EnablementViewModel enablement)
        {
            this.Enablement = enablement;
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.navigationState = navigationState;
            this.groupIdentity = entityIdentity;

            this.Enablement.Init(interviewId, entityIdentity, navigationState);
            this.Title = questionnaire.GroupsWithoutNestedChildren[entityIdentity.Id].Title;

            var groupEntities = questionnaire.GroupsWithoutNestedChildren[entityIdentity.Id].Children;

            var groupModelTypes = new[] {typeof (GroupModel), typeof (RosterModel)};
            var nonQuestionModelTypes = groupModelTypes.Concat(new[] {typeof (StaticTextModel)});

            var groupQuestions = groupEntities.Where(x => !nonQuestionModelTypes.Contains(x.ModelType));

            this.QuestionsCount = groupQuestions.Count();
            this.SubgroupsCount = groupEntities.Count(x => groupModelTypes.Contains(x.ModelType));
            this.AnsweredQuestionsCount = interview.Answers.Values.Count(x => groupQuestions.Any(y => y.Id == x.Id) &&
                                                                              x.RosterVector.SequenceEqual(
                                                                                  entityIdentity.RosterVector) &&
                                                                              x.IsAnswered);

            this.InvalidAnswersCount = 0;

            this.InitStatus();
        }

        private void InitStatus()
        {
            this.Status = GroupStatus.NotStarted;

            if (this.AnsweredQuestionsCount > 0)
                this.Status = GroupStatus.Started;

            if (this.QuestionsCount == this.AnsweredQuestionsCount)
                this.Status = GroupStatus.Completed;

            if (this.InvalidAnswersCount > 0)
                this.Status = GroupStatus.HasInvlidAnswers;
        }

        private void NavigateToGroup()
        {
            this.navigationState.NavigateTo(this.groupIdentity);
        }
    }
}
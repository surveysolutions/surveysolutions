using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public struct GroupState
    {
        public int AnsweredQuestionsCount { get; set; }

        public int SubgroupsCount { get; set; }

        public int QuestionsCount { get; set; }

        public int InvalidAnswersCount { get; set; }

        public GroupStatus Status { get; set; }

    }

    public class GroupViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly AnswerNotifier answerNotifier;

        private NavigationState navigationState;
        private Identity groupIdentity;

        public EnablementViewModel Enablement { get; private set; }
        public string Title { get; private set; }

        public GroupState GroupState
        {
            get { return this.groupState; }
            private set { this.groupState = value; this.RaisePropertyChanged();}
        }

        public bool IsStarted
        {
            get { return this.GroupState.Status > GroupStatus.NotStarted; }
        }

        private IMvxCommand navigateToGroupCommand;
        private string interviewId;
        private GroupStatus status;
        private GroupState groupState;

        public IMvxCommand NavigateToGroupCommand
        {
            get { return navigateToGroupCommand ?? (navigateToGroupCommand = new MvxCommand(NavigateToGroup)); }
        }

        public GroupViewModel(
            IStatefulInterviewRepository interviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            EnablementViewModel enablement,
            AnswerNotifier answerNotifier)
        {
            this.Enablement = enablement;
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.answerNotifier = answerNotifier;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.navigationState = navigationState;
            this.groupIdentity = entityIdentity;

            this.Enablement.Init(interviewId, entityIdentity, navigationState);
            this.Title = questionnaire.GroupsWithFirstLevelChildrenAsReferences[entityIdentity.Id].Title;

            this.UpdateStats();

            Guid parentGroupId = questionnaire.GroupsParentIdMap[entityIdentity.Id].Value;
            Identity parentGroupIdentity = new Identity(parentGroupId, entityIdentity.RosterVector.WithoutLast().ToArray());
            IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(parentGroupIdentity);

            this.answerNotifier.Init(questionsToListen.ToArray());
            this.answerNotifier.QuestionAnswered += QuestionAnswered;
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            UpdateStats();
        }

        private void UpdateStats()
        {
            var interview = this.interviewRepository.Get(interviewId);

            var state = new GroupState();

            state.QuestionsCount = interview.GetQuestionsInGroupCount(groupIdentity);
            state.SubgroupsCount = interview.GetGroupsInGroupCount(groupIdentity);
            state.AnsweredQuestionsCount = interview.GetAnsweredQuestionsCount(groupIdentity);
            state.InvalidAnswersCount = interview.GetInvalidAnswersCount(groupIdentity);

            var newState = GroupStatus.NotStarted;

            if (state.AnsweredQuestionsCount > 0)
                newState = GroupStatus.Started;

            if (state.QuestionsCount == state.AnsweredQuestionsCount)
                newState = GroupStatus.Completed;

            if (state.InvalidAnswersCount > 0)
                newState = GroupStatus.StartedInvalid;

            if (state.InvalidAnswersCount > 0 && state.QuestionsCount == state.AnsweredQuestionsCount)
                newState = GroupStatus.CompletedInvalid;

            state.Status = newState;
            this.GroupState = state;
        }

        private void NavigateToGroup()
        {
            this.navigationState.NavigateTo(this.groupIdentity);
        }
    }
}
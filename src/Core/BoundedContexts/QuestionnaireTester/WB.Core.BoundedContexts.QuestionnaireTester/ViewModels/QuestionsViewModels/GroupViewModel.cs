using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GroupViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
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

        public Identity Identity
        {
            get{ return groupIdentity; }
        }

        private IMvxCommand navigateToGroupCommand;
        private string interviewId;
        private GroupState groupState;

        public IMvxCommand NavigateToGroupCommand
        {
            get { return navigateToGroupCommand ?? (navigateToGroupCommand = new MvxCommand(async () => await NavigateToGroup())); }
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
            var interview = this.interviewRepository.Get(interviewId);

            Identity groupWithAnswersToMonitor = interview.GetParentGroup(entityIdentity);

            this.Init(interviewId, entityIdentity, groupWithAnswersToMonitor, navigationState);
        }

        public void Init(string interviewId, Identity entityIdentity, Identity groupWithAnswersToMonitor, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.navigationState = navigationState;
            this.groupIdentity = entityIdentity;

            this.Enablement.Init(interviewId, entityIdentity, navigationState);
            this.Title = questionnaire.GroupsWithFirstLevelChildrenAsReferences[entityIdentity.Id].Title;

            this.UpdateStats();

            if (groupWithAnswersToMonitor != null)
            {
                IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(groupWithAnswersToMonitor);
                this.answerNotifier.Init(questionsToListen.ToArray());
                this.answerNotifier.QuestionAnswered += QuestionAnswered;
            }
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            UpdateStats();
        }

        private void UpdateStats()
        {
            var interview = this.interviewRepository.Get(interviewId);

            var state = new GroupState(this.groupIdentity);
            state.UpdateSelfFromGroupModelRecursively(interview);

            this.GroupState = state;
        }

        private async Task NavigateToGroup()
        {
            await this.navigationState.NavigateTo(this.groupIdentity);
        }
    }
}
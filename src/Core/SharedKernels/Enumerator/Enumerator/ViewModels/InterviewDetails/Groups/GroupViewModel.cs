using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class GroupViewModel : MvxNotifyPropertyChanged, ILiteEventHandler<RosterInstancesTitleChanged>, IInterviewEntityViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly AnswerNotifier answerNotifier;
        private string interviewId;

        private NavigationState navigationState;
        private Identity groupIdentity;

        public EnablementViewModel Enablement { get; private set; }
        public string Title { get; private set; }
        public bool IsRoster { get; private set; }

        private string rosterTitle;
        public string RosterTitle
        {
            get { return rosterTitle; }
            set
            {
                this.rosterTitle = value;
                this.RaisePropertyChanged();
            }
        }

        private readonly GroupStateViewModel groupState;
        readonly ILiteEventRegistry eventRegistry;

        public GroupStateViewModel GroupState
        {
            get { return this.groupState; }
        }

        public bool IsStarted
        {
            get { return this.GroupState.Status > GroupStatus.NotStarted; }
        }

        public Identity Identity
        {
            get{ return this.groupIdentity; }
        }

        private IMvxCommand navigateToGroupCommand;
        public IMvxCommand NavigateToGroupCommand
        {
            get { return this.navigateToGroupCommand ?? (this.navigateToGroupCommand = new MvxCommand(async () => await this.NavigateToGroupAsync())); }
        }

        public GroupViewModel(
            IStatefulInterviewRepository interviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            EnablementViewModel enablement,
            AnswerNotifier answerNotifier,
            GroupStateViewModel groupState,
            ILiteEventRegistry eventRegistry)
        {
            this.Enablement = enablement;
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.answerNotifier = answerNotifier;
            this.groupState = groupState;
            this.eventRegistry = eventRegistry;
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

            this.eventRegistry.Subscribe(this, interviewId);

            this.Enablement.Init(interviewId, entityIdentity, navigationState);
            this.GroupState.Init(interviewId, entityIdentity);

            var groupModel = questionnaire.GroupsWithFirstLevelChildrenAsReferences[entityIdentity.Id];
            this.Title = groupModel.Title;
            this.RosterTitle = interview.GetRosterTitle(entityIdentity);
            this.IsRoster = groupModel is RosterModel;

            if (groupWithAnswersToMonitor != null)
            {
                IEnumerable<Identity> questionsToListen = interview.GetChildQuestions(groupWithAnswersToMonitor);
                this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());
                this.answerNotifier.QuestionAnswered += this.QuestionAnswered;
            }
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            this.GroupState.UpdateFromModel();
            this.RaisePropertyChanged(() => this.GroupState);
        }

        private async Task NavigateToGroupAsync()
        {
            await this.navigationState.NavigateToAsync(this.groupIdentity);
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            if (!this.IsRoster) return;

            foreach (var changedInstance in @event.ChangedInstances.Where(changedInstance => this.Identity.Equals(changedInstance.RosterInstance.GetIdentity())))
            {
                this.RosterTitle = changedInstance.Title;
            }
        }
    }
}
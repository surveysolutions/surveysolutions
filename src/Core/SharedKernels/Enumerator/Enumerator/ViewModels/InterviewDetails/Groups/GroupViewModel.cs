using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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
    public class GroupViewModel : MvxNotifyPropertyChanged, ILiteEventHandler<RosterInstancesTitleChanged>, IInterviewEntityViewModel,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly AnswerNotifier answerNotifier;
        private string interviewId;

        private NavigationState navigationState;
        private NavigationIdentity groupIdentity;

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
            get{ return this.groupIdentity.TargetGroup; }
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

            this.Init(interviewId, new NavigationIdentity(entityIdentity), groupWithAnswersToMonitor, navigationState);
        }

        public void Init(string interviewId, NavigationIdentity navigationIdentity, Identity groupWithAnswersToMonitor, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.navigationState = navigationState;
            this.groupIdentity = navigationIdentity;

            this.eventRegistry.Subscribe(this, interviewId);

            if (navigationIdentity.ScreenType != ScreenType.Group)
            {
                this.GroupState.Init(interviewId, null, navigationIdentity.ScreenType);
                return;
            }

            GroupModel groupModel;
            if (!questionnaire.GroupsWithFirstLevelChildrenAsReferences.TryGetValue(navigationIdentity.TargetGroup.Id, out groupModel))
                throw new InvalidOperationException("Group with identity {0} don't found".FormatString(navigationIdentity.TargetGroup));

            this.Enablement.Init(interviewId, navigationIdentity.TargetGroup, navigationState);
            this.GroupState.Init(interviewId, navigationIdentity.TargetGroup);

            this.Title = groupModel.Title;
            this.RosterTitle = interview.GetRosterTitle(navigationIdentity.TargetGroup);
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
            this.GroupState.UpdateFromGroupModel();
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

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this, interviewId);

            this.answerNotifier.QuestionAnswered -= this.QuestionAnswered;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class GroupViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        IInterviewEntityViewModel,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly AnswerNotifier answerNotifier;

        private string interviewId;
        private bool isRoster;
        private NavigationState navigationState;
        protected Identity groupIdentity;

        public EnablementViewModel Enablement { get; }

        public DynamicTextViewModel GroupTitle { get; }

        private string rosterInstanceTitle;
        public string RosterInstanceTitle
        {
            get { return this.rosterInstanceTitle; }
            set
            {
                this.rosterInstanceTitle = value;
                this.RaisePropertyChanged();
            }
        }

        private readonly ILiteEventRegistry eventRegistry;

        public GroupStateViewModel GroupState { get; }

        public bool IsStarted => this.GroupState.Status > GroupStatus.NotStarted;

        public Identity Identity => this.groupIdentity;

        public IMvxCommand NavigateToGroupCommand => new MvxCommand(this.NavigateToGroup);

        protected GroupViewModel()
        {
        }

        public GroupViewModel(
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            EnablementViewModel enablement,
            AnswerNotifier answerNotifier,
            GroupStateViewModel groupState,
            DynamicTextViewModel dynamicTextViewModel,
            ILiteEventRegistry eventRegistry)
        {
            this.Enablement = enablement;
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.answerNotifier = answerNotifier;
            this.GroupState = groupState;
            this.GroupTitle = dynamicTextViewModel;
            this.eventRegistry = eventRegistry;
        }

        public virtual void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            var interview = this.interviewRepository.Get(interviewId);

            Identity groupWithAnswersToMonitor = interview.GetParentGroup(entityIdentity);

            this.interviewId = interviewId;
            var statefulInterview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(statefulInterview.QuestionnaireIdentity, statefulInterview.Language);

            this.isRoster = questionnaire.IsRosterGroup(entityIdentity.Id);
            this.navigationState = navigationState;
            this.groupIdentity = entityIdentity;

            this.eventRegistry.Subscribe(this, interviewId);

            if (!questionnaire.HasGroup(entityIdentity.Id))
            {
                throw new InvalidOperationException($"Questionnaire {statefulInterview.QuestionnaireIdentity} has no group with id {entityIdentity.Id}");
            }

            this.Enablement.Init(interviewId, entityIdentity);
            this.GroupState.Init(interviewId, entityIdentity);

            this.GroupTitle.Init(interviewId, entityIdentity);
            this.RosterInstanceTitle = statefulInterview.GetRosterTitle(entityIdentity);
            
            if (groupWithAnswersToMonitor != null)
            {
                IEnumerable<Identity> questionsToListen = statefulInterview.GetChildQuestions(groupWithAnswersToMonitor);
                this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());
                this.answerNotifier.QuestionAnswered += this.QuestionAnswered;
            }
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            this.GroupState.UpdateFromGroupModel();
            this.RaisePropertyChanged(() => this.GroupState);
        }

        private void NavigateToGroup() => this.navigationState.NavigateTo(NavigationIdentity.CreateForGroup(this.groupIdentity));

        public void Handle(RosterInstancesTitleChanged @event)
        {
            if (!this.isRoster) return;

            var changedInstance =
                @event.ChangedInstances.SingleOrDefault(x => this.Identity.Equals(x.RosterInstance.GetIdentity()));

            if (changedInstance != null)
                this.RosterInstanceTitle = changedInstance.Title;
        }

        public virtual void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.answerNotifier.QuestionAnswered -= this.QuestionAnswered;
            this.answerNotifier.Dispose();
            this.GroupTitle.Dispose();
            this.Enablement.Dispose();
        }
    }
}
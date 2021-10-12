using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class GroupViewModel : MvxNotifyPropertyChanged,
        IViewModelEventHandler<RosterInstancesTitleChanged>,
        IInterviewEntityViewModel,
        IDisposable
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly AnswerNotifier answerNotifier;
        private readonly IGroupStateCalculationStrategy groupStateCalculationStrategy;

        private string interviewId;
        private bool isRoster;
        private NavigationState navigationState;
        protected Identity groupIdentity;

        public EnablementViewModel Enablement { get; }

        public DynamicTextViewModel GroupTitle { get; }

        private string rosterInstanceTitle;

        private bool hasCustomTitle;

        public string RosterInstanceTitle
        {
            get => this.rosterInstanceTitle;
            set => this.RaiseAndSetIfChanged(ref rosterInstanceTitle, value);
        }

        private readonly IViewModelEventRegistry eventRegistry;
        private readonly ICommandService commandService;

        private GroupStatus status;
        public GroupStatus Status
        {
            get => this.status;
            protected set => this.RaiseAndSetIfChanged(ref this.status, value);
        }
        
        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled;
            protected set => this.RaiseAndSetIfChanged(ref this.isEnabled, value);
        }



        public Identity Identity => this.groupIdentity;

        public IMvxCommand NavigateToGroupCommand => new MvxAsyncCommand(this.NavigateToGroup);

        protected GroupViewModel()
        {
        }

        public GroupViewModel(
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            EnablementViewModel enablement,
            AnswerNotifier answerNotifier,
            IGroupStateCalculationStrategy groupStateCalculationStrategy,
            DynamicTextViewModel dynamicTextViewModel,
            IViewModelEventRegistry eventRegistry,
            ICommandService commandService)
        {
            this.Enablement = enablement;
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.answerNotifier = answerNotifier;
            this.groupStateCalculationStrategy = groupStateCalculationStrategy;
            this.GroupTitle = dynamicTextViewModel;
            this.eventRegistry = eventRegistry;
            this.commandService = commandService;
        }

        public virtual void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.groupIdentity = entityIdentity;
            this.interviewId = interviewId;
            this.navigationState = navigationState;

            var statefulInterview = this.interviewRepository.Get(interviewId);

            Identity groupWithAnswersToMonitor = statefulInterview.GetParentGroup(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(statefulInterview.QuestionnaireIdentity, statefulInterview.Language);

            this.isRoster = questionnaire.IsRosterGroup(entityIdentity.Id);

            if (!questionnaire.HasGroup(entityIdentity.Id))
            {
                throw new InvalidOperationException($"Questionnaire {statefulInterview.QuestionnaireIdentity} has no group with id {entityIdentity.Id}");
            }

            this.Enablement.Init(interviewId, entityIdentity);
            this.Status = this.groupStateCalculationStrategy.CalculateDetailedStatus(groupIdentity, statefulInterview, questionnaire);

            this.GroupTitle.Init(interviewId, entityIdentity);
            this.hasCustomTitle = questionnaire.HasCustomRosterTitle(entityIdentity.Id);
            this.RosterInstanceTitle = hasCustomTitle ? statefulInterview.GetRosterTitle(entityIdentity) : null; 
            
            if (groupWithAnswersToMonitor != null)
            {
                //subscribe to all interview answer events
                //plain rosters could be created with children
                //only visible questions could produce answer events so no harm
                this.answerNotifier.Init(this.interviewId); 
                this.answerNotifier.QuestionAnswered += this.QuestionAnswered;
            }

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            var statefulInterview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(statefulInterview.QuestionnaireIdentity, statefulInterview.Language);
            this.Status = this.groupStateCalculationStrategy.CalculateDetailedStatus(groupIdentity, statefulInterview, questionnaire);
            this.IsEnabled = true;
        }

        private async Task NavigateToGroup()
        {
            var waitOnCommand = this.commandService.WaitOnCommandAsync();
            var isCommandReceived = await Task.WhenAny(waitOnCommand, Task.Delay(100)) == waitOnCommand;

            if (isCommandReceived || this.commandService.HasPendingCommands)
            {
                IsEnabled = false;
                return;
            }
            
            await this.navigationState.NavigateTo(NavigationIdentity.CreateForGroup(this.groupIdentity));
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            if (!this.isRoster || this.hasCustomTitle) return;

            var rosterIdentity = RosterInstance.CreateFromIdentity(this.Identity);
            var changedInstance = @event.ChangedInstances.SingleOrDefault(x => rosterIdentity.Equals(x.RosterInstance));

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

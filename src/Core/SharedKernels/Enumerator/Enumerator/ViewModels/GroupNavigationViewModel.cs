using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class GroupNavigationViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IAsyncViewModelEventHandler<GroupsEnabled>,
        IAsyncViewModelEventHandler<GroupsDisabled>,
        IAsyncViewModelEventHandler<RosterInstancesTitleChanged>,
        IDisposable
    {

        public class GroupStatistics
        {
            public int EnabledQuestionsCount { get; set; }
            public int SubgroupsCount { get; set; }
            public int AnsweredQuestionsCount { get; set; }
            public int UnansweredQuestionsCount { get; set; }
            public int InvalidAnswersCount { get; set; }
        }

        private string interviewId;
        private bool isRoster;
        private QuestionnaireIdentity questionnaireIdentity;
        private NavigationState navigationState;

        public Identity Identity { get; private set; }
        public NavigationGroupType NavigationGroupType { get; private set; }

        private Identity groupOrSectionToNavigateIdentity;
        private readonly List<Identity> listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection = new List<Identity>();

        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly ICommandService commandService;
        private readonly IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher;
        private readonly AnswerNotifier answerNotifier;


        private GroupStateViewModel navigateToGroupState;
        public GroupStateViewModel NavigateToGroupState
        {
            get { return this.navigateToGroupState; }
            set { this.RaiseAndSetIfChanged(ref this.navigateToGroupState, value); }
        }

        private bool isEnabled = true;

        public bool IsEnabled
        {
            get => isEnabled;
            protected set => this.RaiseAndSetIfChanged(ref this.isEnabled, value);
        }
        
        public DynamicTextViewModel Title { get; }

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

        private string navigationItemTitle;
        public string NavigationItemTitle
        {
            get { return this.navigationItemTitle; }
            set { this.RaiseAndSetIfChanged(ref this.navigationItemTitle, value); }
        }

        public IMvxAsyncCommand NavigateCommand => new MvxAsyncCommand(this.NavigateAsync);

        protected GroupNavigationViewModel() {}

        public GroupNavigationViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelEventRegistry eventRegistry,
            IInterviewViewModelFactory interviewViewModelFactory,
            ICommandService commandService, 
            DynamicTextViewModel title,
            AnswerNotifier answerNotifier)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.commandService = commandService;
            this.Title = title;
            this.answerNotifier = answerNotifier;
            this.mvxMainThreadDispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>(); ;
        }

        public virtual void Init(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.Identity = groupIdentity;
            this.navigationState = navigationState;

            var interview = this.interviewRepository.Get(interviewId);
            this.questionnaireIdentity = interview.QuestionnaireIdentity;

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            if (groupIdentity == null || !questionnaire.GetParentGroup(groupIdentity.Id).HasValue)
            {
                this.SetNextEnabledSection();
            }
            else
            {
                this.groupOrSectionToNavigateIdentity = interview.GetParentGroup(this.Identity);
                this.isRoster = questionnaire.IsRosterGroup(this.groupOrSectionToNavigateIdentity.Id);
                this.RosterInstanceTitle = interview.GetRosterTitle(this.groupOrSectionToNavigateIdentity);
                this.NavigationGroupType = NavigationGroupType.ToParentGroup;
            }

            this.SetNavigationItemTitle();
            this.SetGroupsStates();
            
            var questionsToListen = groupIdentity == null
                ? questionnaire.GetPrefilledQuestions().Select(x => new Identity(x, RosterVector.Empty)).ToArray()
                :interview.GetChildQuestions(groupIdentity);
            
            this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());
            this.answerNotifier.QuestionAnswered += this.QuestionAnswered;

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            this.NavigateToGroupState?.UpdateFromGroupModel();
            this.RaisePropertyChanged(() => this.NavigateToGroupState);
            this.IsEnabled = true;
        }

        private void SetGroupsStates()
        {
            switch (this.NavigationGroupType)
            {
                case NavigationGroupType.ToParentGroup:
                case NavigationGroupType.Section:
                    this.navigateToGroupState = this.interviewViewModelFactory.GetNew<GroupStateViewModel>();
                    break;
                case NavigationGroupType.LastSection:
                    this.navigateToGroupState = this.interviewViewModelFactory.GetNew<InterviewStateViewModel>();
                    break;
            }

            this.navigateToGroupState.Init(this.interviewId, this.groupOrSectionToNavigateIdentity);
            this.RaisePropertyChanged(() => this.NavigateToGroupState);
        }
 
        private void SetNextEnabledSection()
        {
            this.groupOrSectionToNavigateIdentity = null;
            this.listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection.Clear();

            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.questionnaireIdentity, interview.Language);

            var sections = questionnaire.GetAllSections().ToList();

            int currentSectionIndex = this.Identity == null ? 0 :sections.FindIndex(x => x == this.Identity.Id);
            int startSectionIndex = !questionnaire.IsCoverPageSupported && this.Identity == null 
                ? currentSectionIndex 
                : currentSectionIndex + 1;
            for (int sectionIndex = startSectionIndex; sectionIndex < sections.Count; sectionIndex++)
            {
                var nextSectionIdentity = new Identity(sections[sectionIndex], RosterVector.Empty);

                if (!this.CanNavigateToSection(nextSectionIdentity))
                {
                    this.listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection.Add(nextSectionIdentity);
                    continue;
                }

                this.groupOrSectionToNavigateIdentity = nextSectionIdentity;
                break;
            }

            this.NavigationGroupType = this.groupOrSectionToNavigateIdentity == null ? NavigationGroupType.LastSection : NavigationGroupType.Section;
        }

        private void SetNavigationItemTitle()
        {
            switch (this.NavigationGroupType)
            {
                case NavigationGroupType.ToParentGroup:
                case NavigationGroupType.Section:
                    this.Title.Dispose();
                    this.Title.Init(this.interviewId, this.groupOrSectionToNavigateIdentity);
                    return;
                case NavigationGroupType.LastSection:
                    this.Title.InitAsStatic(UIResources.Interview_CompleteScreen_ButtonText);
                    return;
                default:
                    return;
            }
        }

        private bool CanNavigateToSection(Identity group)
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            return interview.HasGroup(group) && interview.IsEnabled(group);
        }

        private async Task NavigateAsync()
        {
            // focus out event is fired after navigation button click event, so we cannot accept answer before leaving a section. This delay should force command to be put into the queue before navigation
            var waitOnCommand = this.commandService.WaitOnCommandAsync();
            var isCommandReceived = await Task.WhenAny(waitOnCommand, Task.Delay(100)) == waitOnCommand;

            if (isCommandReceived || this.commandService.HasPendingCommands)
            {
                IsEnabled = false;
                return;
            }
                
            await this.commandService.WaitPendingCommandsAsync().ConfigureAwait(false);
            switch (this.NavigationGroupType)
            {
                case NavigationGroupType.ToParentGroup:
                    await this.navigationState.NavigateTo(
                        NavigationIdentity.CreateForGroup(this.groupOrSectionToNavigateIdentity,
                            anchoredElementIdentity: this.Identity));
                    break;
                case NavigationGroupType.Section:
                    await this.navigationState.NavigateTo(
                        NavigationIdentity.CreateForGroup(this.groupOrSectionToNavigateIdentity));
                    break;
                case NavigationGroupType.LastSection:
                    await this.navigationState.NavigateTo(NavigationIdentity.CreateForCompleteScreen());
                    break;
            }
        }

        private async Task UpdateNavigation()
        {
            await this.mvxMainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.SetNextEnabledSection();
                this.SetNavigationItemTitle();
                this.SetGroupsStates();
            });
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            
            this.Title.Dispose();
            this.answerNotifier.QuestionAnswered -= this.QuestionAnswered;
            this.answerNotifier.Dispose();
        }

        public async Task HandleAsync(GroupsEnabled @event)
        {
            if (this.NavigationGroupType == NavigationGroupType.ToParentGroup) return;
            if (!this.listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection.Intersect(@event.Groups).Any()) return;

            await this.UpdateNavigation();
        }

        public Task HandleAsync(RosterInstancesTitleChanged @event)
        {
            if (!this.isRoster) return Task.CompletedTask;

            var changedInstance =
                @event.ChangedInstances.SingleOrDefault(x => this.groupOrSectionToNavigateIdentity.Equals(x.RosterInstance.GetIdentity()));

            if (changedInstance != null)
                this.RosterInstanceTitle = changedInstance.Title;

            return Task.CompletedTask;
        }

        public async Task HandleAsync(GroupsDisabled @event)
        {
            if (this.NavigationGroupType == NavigationGroupType.ToParentGroup) return;
            if (!@event.Groups.Contains(this.groupOrSectionToNavigateIdentity)) return;

            await this.UpdateNavigation();
        }
    }
}

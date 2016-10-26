using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class GroupNavigationViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<GroupsEnabled>, 
        ILiteEventHandler<GroupsDisabled>, 
        ILiteEventHandler<RosterInstancesTitleChanged>,
        IDisposable
    {
        private enum NavigationGroupType { Section, LastSection, InsideGroupOrRoster }
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
        private Identity groupOrSectionToNavigateIdentity;
        private NavigationGroupType navigationGroupType;
        private readonly List<Identity> listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection = new List<Identity>();

        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly ICommandService commandService;
        private readonly AnswerNotifier answerNotifier;

        private GroupStateViewModel groupOrSectionToNavigateGroupState;
        public GroupStateViewModel GroupOrSectionToNavigateGroupState
        {
            get { return this.groupOrSectionToNavigateGroupState; }
            set { this.RaiseAndSetIfChanged(ref this.groupOrSectionToNavigateGroupState, value); }
        }

        private GroupStateViewModel groupState;
        public GroupStateViewModel GroupState
        {
            get { return this.groupState; }
            set { this.RaiseAndSetIfChanged(ref this.groupState, value); }
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

        public bool IsRoster
        {
            get { return isRoster; }
            set
            {
                isRoster = value;
                this.RaisePropertyChanged();
            }
        }

        private string navigationItemTitle;
        public string NavigationItemTitle
        {
            get { return this.navigationItemTitle; }
            set { this.RaiseAndSetIfChanged(ref this.navigationItemTitle, value); }
        }

        public IMvxCommand NavigateCommand
        {
            get { return new MvxCommand(async () => await this.NavigateAsync()); }
        }

        protected GroupNavigationViewModel() {}

        public GroupNavigationViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
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
        }

        public virtual void Init(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.Identity = groupIdentity;
            this.navigationState = navigationState;

            this.eventRegistry.Subscribe(this, interviewId);

            var interview = this.interviewRepository.Get(interviewId);
            this.questionnaireIdentity = interview.QuestionnaireIdentity;

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            if (!questionnaire.GetParentGroup(groupIdentity.Id).HasValue)
            {
                this.SetNextEnabledSection();
            }
            else
            {
                this.groupOrSectionToNavigateIdentity = this.GetParentGroupOrRosterIdentity();
                this.IsRoster = questionnaire.IsRosterGroup(this.groupOrSectionToNavigateIdentity.Id);
                this.RosterInstanceTitle = interview.GetRosterTitle(this.groupOrSectionToNavigateIdentity);
                this.navigationGroupType = NavigationGroupType.InsideGroupOrRoster;
            }

            this.SetNavigationItemTitle();
            this.SetGroupsStates();

            var questionsToListen = interview.GetChildQuestions(groupIdentity);
            this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());
            this.answerNotifier.QuestionAnswered += this.QuestionAnswered;
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            this.GroupState.UpdateFromGroupModel();
            this.RaisePropertyChanged(() => this.GroupState);

            this.GroupOrSectionToNavigateGroupState?.UpdateFromGroupModel();
            this.RaisePropertyChanged(() => this.GroupOrSectionToNavigateGroupState);
        }

        private Identity GetParentGroupOrRosterIdentity()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.questionnaireIdentity, interview.Language);

            Guid parentId = questionnaire.GetParentGroup(this.Identity.Id).Value;
            int rosterLevelOfParent = questionnaire.GetRosterLevelForGroup(parentId);

            RosterVector parentRosterVector = this.Identity.RosterVector.Shrink(rosterLevelOfParent);

            return new Identity(parentId, parentRosterVector);
        }

        private void SetGroupsStates()
        {
            switch (this.navigationGroupType)
            {
                case NavigationGroupType.InsideGroupOrRoster:
                case NavigationGroupType.Section:
                    this.groupState = this.interviewViewModelFactory.GetNew<GroupStateViewModel>();
                    this.groupOrSectionToNavigateGroupState = this.interviewViewModelFactory.GetNew<GroupStateViewModel>();
                    this.groupOrSectionToNavigateGroupState.Init(this.interviewId, this.groupOrSectionToNavigateIdentity);
                    break;
                case NavigationGroupType.LastSection:
                    this.groupState = this.interviewViewModelFactory.GetNew<InterviewStateViewModel>();
                    this.groupOrSectionToNavigateGroupState = null;
                    break;
            }

            this.groupState.Init(this.interviewId, this.Identity);

            this.RaisePropertyChanged(() => this.GroupState);
            this.RaisePropertyChanged(() => this.GroupOrSectionToNavigateGroupState);
        }
 
        private void SetNextEnabledSection()
        {
            this.groupOrSectionToNavigateIdentity = null;
            this.listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection.Clear();

            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.questionnaireIdentity, interview.Language);

            var sections = questionnaire.GetAllSections().ToList();

            int currentSectionIndex = sections.FindIndex(x => x == this.Identity.Id);
            for (int sectionIndex = currentSectionIndex + 1; sectionIndex < sections.Count; sectionIndex++)
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

            this.navigationGroupType = this.groupOrSectionToNavigateIdentity == null ? NavigationGroupType.LastSection : NavigationGroupType.Section;
        }

        private void SetNavigationItemTitle()
        {
            switch (this.navigationGroupType)
            {
                case NavigationGroupType.InsideGroupOrRoster:
                case NavigationGroupType.Section:
                    var interview = this.interviewRepository.Get(interviewId);
                    var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
                    var textWithSubstitutions = questionnaire.GetGroupTitle(this.groupOrSectionToNavigateIdentity.Id);

                    this.Title.Dispose();
                    this.Title.Init(this.interviewId, this.groupOrSectionToNavigateIdentity, textWithSubstitutions);

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
            await this.commandService.WaitPendingCommandsAsync().ConfigureAwait(false);
            switch (this.navigationGroupType)
            {
                case NavigationGroupType.InsideGroupOrRoster:
                    this.navigationState.NavigateTo(
                        NavigationIdentity.CreateForGroup(this.groupOrSectionToNavigateIdentity,
                            anchoredElementIdentity: this.Identity));
                    break;
                case NavigationGroupType.Section:
                    this.navigationState.NavigateTo(
                        NavigationIdentity.CreateForGroup(this.groupOrSectionToNavigateIdentity));
                    break;
                case NavigationGroupType.LastSection:
                    this.navigationState.NavigateTo(NavigationIdentity.CreateForCompleteScreen());
                    break;
            }
        }

        private void UpdateNavigation()
        {
            this.SetNextEnabledSection();
            this.SetNavigationItemTitle();
            this.SetGroupsStates();
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            
            this.Title.Dispose();
            this.answerNotifier.QuestionAnswered -= this.QuestionAnswered;
            this.answerNotifier.Dispose();
        }

        public void Handle(GroupsEnabled @event)
        {
            if (this.navigationGroupType == NavigationGroupType.InsideGroupOrRoster) return;
            if (!this.listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection.Intersect(@event.Groups).Any()) return;

            this.UpdateNavigation();
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            if (!this.isRoster) return;

            foreach (var changedInstance in @event.ChangedInstances.Where(changedInstance => this.groupOrSectionToNavigateIdentity.Equals(changedInstance.RosterInstance.GetIdentity())))
            {
                this.RosterInstanceTitle = changedInstance.Title;
            }
        }

        public void Handle(GroupsDisabled @event)
        {
            if (this.navigationGroupType == NavigationGroupType.InsideGroupOrRoster) return;
            if (!@event.Groups.Contains(this.groupOrSectionToNavigateIdentity)) return;

            this.UpdateNavigation();
        }
    }
}
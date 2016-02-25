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
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class GroupNavigationViewModel : MvxNotifyPropertyChanged, ILiteEventHandler<GroupsEnabled>, ILiteEventHandler<GroupsDisabled>, IDisposable
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
        private QuestionnaireIdentity questionnaireIdentity;
        private NavigationState navigationState;

        private Identity currentGroupIdentity;
        private Identity groupOrSectionToNavigateIdentity;
        private NavigationGroupType navigationGroupType;
        private readonly List<Identity> listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection = new List<Identity>();

        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly ICommandService commandService;
        private readonly AnswerNotifier answerNotifier;

        private string QuestionnaireId => this.questionnaireIdentity.ToString();

        private GroupStateViewModel groupState;
        public GroupStateViewModel GroupState
        {
            get { return this.groupState; }
            set { this.RaiseAndSetIfChanged(ref this.groupState, value); }
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
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            IInterviewViewModelFactory interviewViewModelFactory,
            ICommandService commandService, AnswerNotifier answerNotifier)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.commandService = commandService;
            this.answerNotifier = answerNotifier;
        }

        public virtual void Init(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.currentGroupIdentity = groupIdentity;
            this.navigationState = navigationState;

            this.eventRegistry.Subscribe(this, interviewId);

            var interview = this.interviewRepository.Get(interviewId);
            this.questionnaireIdentity = interview.QuestionnaireIdentity;

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);
            if (!questionnaire.GetParentGroup(groupIdentity.Id).HasValue)
            {
                this.SetNextEnabledSection();
            }
            else
            {
                this.groupOrSectionToNavigateIdentity = this.GetParentGroupOrRosterIdentity();
                this.navigationGroupType = NavigationGroupType.InsideGroupOrRoster;
            }

            this.UpdateNavigationItemTitle();
            this.SetGroupState();

            var questionsToListen = interview.GetChildQuestions(groupIdentity);
            this.answerNotifier.Init(this.interviewId, questionsToListen.ToArray());
            this.answerNotifier.QuestionAnswered += this.QuestionAnswered;
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            this.GroupState.UpdateFromGroupModel();
            this.RaisePropertyChanged(() => this.GroupState);
        }

        private Identity GetParentGroupOrRosterIdentity()
        {
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.questionnaireIdentity);

            Guid parentId = questionnaire.GetParentGroup(this.currentGroupIdentity.Id).Value;
            int rosterLevelOfParent = questionnaire.GetRosterLevelForGroup(parentId);

            RosterVector parentRosterVector = this.currentGroupIdentity.RosterVector.Shrink(rosterLevelOfParent);

            return new Identity(parentId, parentRosterVector);
        }

        private void SetGroupState()
        {
            switch (this.navigationGroupType)
            {
                case NavigationGroupType.InsideGroupOrRoster:
                case NavigationGroupType.Section:
                    this.groupState = this.interviewViewModelFactory.GetNew<GroupStateViewModel>();
                    break;
                case NavigationGroupType.LastSection:
                    this.groupState = this.interviewViewModelFactory.GetNew<InterviewStateViewModel>();
                    break;
            }

            this.groupState.Init(this.interviewId, this.currentGroupIdentity);
            this.RaisePropertyChanged(() => this.GroupState);
        }
 
        private void SetNextEnabledSection()
        {
            this.groupOrSectionToNavigateIdentity = null;
            this.listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection.Clear();

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.questionnaireIdentity);

            var sections = questionnaire.GetAllSections().ToList();

            int currentSectionIndex = sections.FindIndex(x => x == this.currentGroupIdentity.Id);
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

        private void UpdateNavigationItemTitle()
        {
            this.NavigationItemTitle = this.GetNavigationItemTitle();
        }

        private string GetNavigationItemTitle()
        {
            switch (this.navigationGroupType)
            {
                case NavigationGroupType.InsideGroupOrRoster:
                    return UIResources.Interview_ParentGroup_ButtonText;
                case NavigationGroupType.LastSection:
                    return UIResources.Interview_CompleteScreen_ButtonText;
                case NavigationGroupType.Section:
                    return UIResources.Interview_NextSection_ButtonText;
                default:
                    return string.Empty;
            }
        }

        private bool CanNavigateToSection(Identity group)
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            return interview.HasGroup(group) && interview.IsEnabled(group);
        }

        private async Task NavigateAsync()
        {
            await this.commandService.WaitPendingCommandsAsync();
            switch (this.navigationGroupType)
            {
                case NavigationGroupType.InsideGroupOrRoster:
                    await this.navigationState.NavigateToAsync(
                        NavigationIdentity.CreateForGroup(this.groupOrSectionToNavigateIdentity,
                            anchoredElementIdentity: this.currentGroupIdentity));
                    break;
                case NavigationGroupType.Section:
                    await this.navigationState.NavigateToAsync(
                        NavigationIdentity.CreateForGroup(this.groupOrSectionToNavigateIdentity));
                    break;
                case NavigationGroupType.LastSection:
                    await this.navigationState.NavigateToAsync(NavigationIdentity.CreateForCompleteScreen());
                    break;
            }
        }

        private void UpdateNavigation()
        {
            this.SetNextEnabledSection();
            this.UpdateNavigationItemTitle();
            this.SetGroupState();
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this, this.interviewId);

            this.answerNotifier.QuestionAnswered -= this.QuestionAnswered;
            this.answerNotifier.Dispose();
        }

        public void Handle(GroupsEnabled @event)
        {
            if (this.navigationGroupType == NavigationGroupType.InsideGroupOrRoster) return;
            if (!this.listOfDisabledSectionBetweenCurrentSectionAndNextEnabledSection.Intersect(@event.Groups).Any()) return;

            this.UpdateNavigation();
        }

        public void Handle(GroupsDisabled @event)
        {
            if (this.navigationGroupType == NavigationGroupType.InsideGroupOrRoster) return;
            if (!@event.Groups.Contains(this.groupOrSectionToNavigateIdentity)) return;

            this.UpdateNavigation();
        }
    }
}
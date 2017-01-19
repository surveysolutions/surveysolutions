using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    [DebuggerDisplay("Title = {Title.PlainText}, Id = {SectionIdentity}")]
    public class SideBarSectionViewModel : MvxNotifyPropertyChanged, ISideBarSectionItem,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IMvxMessenger messenger;
        private readonly ILiteEventRegistry eventRegistry;

        private readonly AnswerNotifier answerNotifier;
        public DynamicTextViewModel Title { get; }

        private string interviewId;
        private Guid[] rostersInGroup;

        public SideBarSectionViewModel(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IMvxMessenger messenger,
            ILiteEventRegistry eventRegistry,
            DynamicTextViewModel dynamicTextViewModel,
            AnswerNotifier answerNotifier)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.messenger = messenger;
            this.eventRegistry = eventRegistry;
            this.answerNotifier = answerNotifier;

            this.Title = dynamicTextViewModel;
        }

        public void Init(string interviewId, Identity sectionIdentity, GroupStateViewModel groupStateViewModel,
            NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.eventRegistry.Subscribe(this, interviewId);

            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            this.SectionIdentity = sectionIdentity;

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            this.rostersInGroup = questionnaire
                    .GetAllUnderlyingChildRosters(this.SectionIdentity.Id).ToArray();

            groupStateViewModel.Init(interviewId, sectionIdentity);

            var interviewTreeGroup = interview.GetGroup(this.SectionIdentity);

            this.ParentIdentity = interviewTreeGroup?.Parent?.Identity;

            this.ParentsIdentities = interviewTreeGroup?.Parents?.Select(section => section?.Identity).ToArray() ??
                Enumerable.Empty<Identity>().ToArray();

            this.NodeDepth = this.ParentsIdentities.Length;
            this.Title.Init(interviewId, this.SectionIdentity);
            this.SideBarGroupState = groupStateViewModel;
            this.NavigationState = navigationState;
            this.NavigationState.ScreenChanged += this.OnScreenChanged;

            this.UpdateHasChildren();
            this.UpdateSelection(navigationState.CurrentGroup);
            
            this.answerNotifier.Init(this.interviewId);
            this.answerNotifier.QuestionAnswered += this.QuestionAnswered;
        }

        private void QuestionAnswered(object sender, EventArgs e)
        {
            this.SideBarGroupState.UpdateFromGroupModel();
            this.RaisePropertyChanged(() => this.SideBarGroupState);
        }

        private void OnScreenChanged(ScreenChangedEventArgs eventArgs) => this.UpdateSelection(eventArgs.TargetGroup);
        
        public GroupStateViewModel SideBarGroupState { get; private set; }

        public NavigationState NavigationState { get; set; }

        public Identity SectionIdentity { get; private set; }
        public Identity[] ParentsIdentities { get; private set; }
        public Identity ParentIdentity { get; private set; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.RaiseAndSetIfChanged(ref this.isSelected, value); }
        }

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return this.isCurrent; }
            set { this.RaiseAndSetIfChanged(ref this.isCurrent, value); }
        }

        private bool hasChildren;
        public bool HasChildren
        {
            get { return this.hasChildren; }
            set { this.RaiseAndSetIfChanged(ref this.hasChildren, value); }
        }

        private bool expanded;
        public bool Expanded
        {
            get { return this.expanded; }
            set { this.RaiseAndSetIfChanged(ref this.expanded, value); }
        }

        public int NodeDepth { get; set; }
        public ICommand NavigateToSectionCommand => new MvxCommand(this.NavigateToSection);

        public ICommand ToggleCommand => new MvxCommand(this.Toggle);

        private void Toggle()
        {
            if(this.Expanded)
                this.messenger.Publish(new SideBarSectionCollapseMessage(this, this.SectionIdentity));
            else
                this.messenger.Publish(new SideBarSectionExpandMessage(this, this.SectionIdentity));

            this.Expanded = !this.Expanded;
        }

        private void NavigateToSection()
        {
            this.messenger.Publish(new SectionChangeMessage(this));
            this.NavigationState.NavigateTo(NavigationIdentity.CreateForGroup(this.SectionIdentity));
        }

        private void UpdateSelection(Identity targetGroup)
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            var isParentSelected = targetGroup != null && (interview.GetGroup(targetGroup)
                .Parents?.Any(x => x.Identity == this.SectionIdentity) ?? false);

            this.IsCurrent = this.SectionIdentity.Equals(targetGroup);
            this.Expanded = this.IsSelected = this.IsCurrent || isParentSelected;
        }

        private void UpdateHasChildren()
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            this.HasChildren = interview.GetEnabledSubgroups(this.SectionIdentity).Any();
        }

        private void UpdateSubGroups(Identity[] addedSubGroups)
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            if (addedSubGroups.Any(x => interview.GetGroup(x)?.Parent?.Identity == this.SectionIdentity))
                this.messenger.Publish(new SideBarSectionUpdateMessage(this, this.SectionIdentity));
        }

        public void Handle(RosterInstancesAdded @event)
        {
            if (@event.Instances.Select(rosterInstance => rosterInstance.GroupId).Any(rosterId => this.rostersInGroup.Contains(rosterId)))
                this.UpdateHasChildren();

            if (!this.Expanded) return;

            this.UpdateSubGroups(@event.Instances.Select(x => x.GetIdentity()).ToArray());
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            if (@event.Instances.Select(rosterInstance => rosterInstance.GroupId).Any(rosterId => this.rostersInGroup.Contains(rosterId)))
                this.UpdateHasChildren();
        }

        public void Handle(GroupsEnabled @event)
        {
            if (!this.HasChildren && @event.Groups.Any(g => this.rostersInGroup.Contains(g.Id)))
                this.UpdateHasChildren();

            if (this.Expanded)
            {
                this.UpdateSubGroups(@event.Groups);
            }
        }

        public void Handle(GroupsDisabled @event)
        {
            if (@event.Groups.Select(group => group.Id).Any(groupId => this.rostersInGroup.Contains(groupId)))
                this.UpdateHasChildren();
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);

            this.answerNotifier.QuestionAnswered -= this.QuestionAnswered;
            this.answerNotifier.Dispose();

            this.NavigationState.ScreenChanged -= this.OnScreenChanged;
            this.Title.Dispose();
        }
    }
}
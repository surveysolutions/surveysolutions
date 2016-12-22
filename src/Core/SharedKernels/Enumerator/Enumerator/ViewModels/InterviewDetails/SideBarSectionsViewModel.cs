using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SideBarSectionsViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<GroupsEnabled>,
        IDisposable
    {
        private readonly IMvxMessenger messenger;
        private readonly ILiteEventRegistry eventRegistry;
        private NavigationState navigationState;

        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ISideBarSectionViewModelsFactory modelsFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private string interviewId;
        private List<Identity> sectionIdentities;

        private readonly MvxSubscriptionToken sectionExpandSubscriptionToken;
        private readonly MvxSubscriptionToken sectionCollapseSubscriptionToken;
        private readonly MvxSubscriptionToken sectionRemoveSubscriptionToken;
        private readonly MvxSubscriptionToken sectionUpdateSubscriptionToken;

        private ObservableRangeCollection<ISideBarItem> allVisibleSections;
        public ObservableRangeCollection<ISideBarItem> AllVisibleSections
        {
            get { return this.allVisibleSections; }
            set { this.RaiseAndSetIfChanged(ref this.allVisibleSections, value); }
        }

        private IEnumerable<ISideBarSectionItem> sectionViewModels => this.AllVisibleSections.OfType<ISideBarSectionItem>();

        public SideBarSectionsViewModel(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            ISideBarSectionViewModelsFactory modelsFactory,
            IMvxMessenger messenger,
            ILiteEventRegistry eventRegistry)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.modelsFactory = modelsFactory;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.messenger = messenger;
            this.eventRegistry = eventRegistry;
            this.sectionExpandSubscriptionToken = this.messenger.Subscribe<SideBarSectionExpandMessage>(OnSideBarSectionExpanded);
            this.sectionCollapseSubscriptionToken = this.messenger.Subscribe<SideBarSectionCollapseMessage>(OnSideBarSectionCollapsed);
            this.sectionRemoveSubscriptionToken = this.messenger.Subscribe<SideBarSectionRemoveMessage>(OnSideBarSectionRemoved);
            this.sectionUpdateSubscriptionToken = this.messenger.Subscribe<SideBarSectionUpdateMessage>(OnSideBarSectionUpdated);
        }

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException(nameof(navigationState));
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));

            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.interviewId = interviewId;
            this.navigationState = navigationState;
            this.navigationState.ScreenChanged += this.OnScreenChanged;

            this.eventRegistry.Subscribe(this);
            
            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.sectionIdentities = questionnaire.GetAllSections()
                .Select(sectionId => Identity.Create(sectionId, RosterVector.Empty))
                .ToList();

            this.AllVisibleSections = new ObservableRangeCollection<ISideBarItem>(this.GetSectionsWithCoverAndComplete());
        }

        private void OnScreenChanged(ScreenChangedEventArgs e)
        {
            var enabledSectionIdentites = this.GetEnabledSectionIdentites().ToList();
            var expandedSectionIdentities = this.GetExpandedSectionsIdentities(e.TargetGroup).ToList();

            var removedSubsections = this.sectionViewModels.Where(
                sectionViewModel =>
                    !enabledSectionIdentites.Contains(sectionViewModel.SectionIdentity) &&
                    !expandedSectionIdentities.Contains(sectionViewModel.SectionIdentity)).ToList();

            this.AllVisibleSections.RemoveRange(removedSubsections);

            foreach (var expandedGroup in expandedSectionIdentities)
                this.ExpandSection(expandedGroup, false);
        }
        
        private void OnSideBarSectionUpdated(SideBarSectionUpdateMessage e)
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            var updatedGroupViewModel = this.sectionViewModels.FirstOrDefault(x => x.SectionIdentity == e.UpdatedGroup);
            if (updatedGroupViewModel == null) return;

            var indexOfUpdatedGroup = this.AllVisibleSections.IndexOf(updatedGroupViewModel);

            var enabledSubSections = interview.GetEnabledSubgroups(e.UpdatedGroup).ToList();

            foreach (var sectionIdentity in enabledSubSections)
            {
                if (this.sectionViewModels.Any(section => section.SectionIdentity == sectionIdentity))
                    continue;

                var sectionViewModel = this.modelsFactory.BuildSectionItem(sectionIdentity,
                    this.navigationState, this.interviewId);

                var indexOfSubsection = indexOfUpdatedGroup + enabledSubSections.IndexOf(sectionIdentity) + 1;
                this.AllVisibleSections.Insert(indexOfSubsection, sectionViewModel);
            }
        }

        private void OnSideBarSectionRemoved(SideBarSectionRemoveMessage e)
            => this.DisposeAndRemoveSection(e.RemovedGroup);

        private void OnSideBarSectionCollapsed(SideBarSectionCollapseMessage e)
            => this.CollapseSection(e.CollapsedGroup);

        private void OnSideBarSectionExpanded(SideBarSectionExpandMessage e) 
            => this.ExpandSection(e.ExpandedGroup);

        private void CollapseSection(Identity collapsedSection)
        {
            var collapsedViewModels = this.sectionViewModels.Where(
                sectionViewModel => sectionViewModel.ParentsIdentities.Contains(collapsedSection));

            this.AllVisibleSections.RemoveRange(collapsedViewModels);
        }

        private void ExpandSection(Identity expandedSection, bool force = true)
        {
            var expandedGroupViewModel = this.sectionViewModels
                .FirstOrDefault(section => section.SectionIdentity == expandedSection);

            if (expandedGroupViewModel == null) return;
            if (!force && expandedGroupViewModel.Expanded) return;

            var indexOfExpandedGroup = this.AllVisibleSections.IndexOf(expandedGroupViewModel);

            this.AllVisibleSections.InsertRange(indexOfExpandedGroup + 1, this.GetSubSections(expandedSection));
        }

        private void DisposeAndRemoveSection(Identity sectionIdentity)
        {
            var sectionWithChildrenViewModels = this.sectionViewModels
                .Where(section =>
                    section.SectionIdentity == sectionIdentity || section.ParentsIdentities.Contains(sectionIdentity))
                .ToList();

            this.AllVisibleSections.RemoveRange(sectionWithChildrenViewModels);
        }

        private void AddSection(Identity addedSection)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<ISideBarItem> GetSubSections(Identity expandedGroup)
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            foreach (var subSectionIdentity in interview.GetEnabledSubgroups(expandedGroup))
            {
                yield return this.modelsFactory.BuildSectionItem(subSectionIdentity,
                    this.navigationState, this.interviewId);
            }
        }

        private IEnumerable<Identity> GetEnabledSectionIdentites()
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            return this.sectionIdentities.Where(interview.IsEnabled);
        }

        private IEnumerable<Identity> GetExpandedSectionsIdentities(Identity expandedGroupIdentity)
        {
            if (expandedGroupIdentity == null) yield break;

            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            var parentIdentities = interview.GetGroup(expandedGroupIdentity)
                .Parents?.Select(parentGroup => parentGroup.Identity);

            if (parentIdentities != null)
            {
                foreach (var parentIdentity in parentIdentities)
                {
                    yield return parentIdentity;
                }
            }

            yield return expandedGroupIdentity;
        }

        private IEnumerable<ISideBarItem> GetSectionsWithCoverAndComplete()
        {
            yield return this.modelsFactory.BuildCoverItem(this.navigationState);

            foreach (var sectionIdentity in this.GetEnabledSectionIdentites())
                yield return this.modelsFactory.BuildSectionItem(sectionIdentity, this.navigationState, this.interviewId);

            yield return this.modelsFactory.BuildCompleteItem(this.navigationState, this.interviewId);
        }

        public void Handle(GroupsEnabled @event)
        {
            var addedSections = @event.Groups.Intersect(this.sectionIdentities);
            foreach (var addedSection in addedSections)
                this.AddSection(addedSection);
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);

            this.messenger.Unsubscribe<SideBarSectionCollapseMessage>(this.sectionCollapseSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionExpandMessage>(this.sectionExpandSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionRemoveMessage>(this.sectionRemoveSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionUpdateMessage>(this.sectionUpdateSubscriptionToken);

            this.AllVisibleSections.ForEach(x => x.Dispose());
        }
    }
}
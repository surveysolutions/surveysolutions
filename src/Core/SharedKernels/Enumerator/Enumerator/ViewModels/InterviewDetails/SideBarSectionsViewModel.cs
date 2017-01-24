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
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SideBarSectionsViewModel : MvxViewModel,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>,
        ILiteEventHandler<RosterInstancesRemoved>,
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
        private readonly MvxSubscriptionToken sectionUpdateSubscriptionToken;

        private readonly SynchronizedList<ISideBarItem> sectionItems = new SynchronizedList<ISideBarItem>();

        private ObservableRangeCollection<ISideBarItem> allVisibleSections;
        public ObservableRangeCollection<ISideBarItem> AllVisibleSections
        {
            get { return this.allVisibleSections; }
            set
            {
                this.allVisibleSections = value;
                this.RaiseAndSetIfChanged(ref this.allVisibleSections, value);
            }
        }

        private IEnumerable<ISideBarSectionItem> sectionViewModels => this.sectionItems.OfType<ISideBarSectionItem>();

        private readonly object sectionViewModelsLock = new object();


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
            this.sectionUpdateSubscriptionToken = this.messenger.Subscribe<SideBarSectionUpdateMessage>(OnSideBarSectionUpdated);
        }
        
        public class SavedState
        {
            public string InterviewId { get; set; }
        }

        public SavedState SaveState() => new SavedState {InterviewId = this.interviewId};
        public void ReloadState(SavedState savedState) => this.interviewId = savedState.InterviewId;

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

            foreach (var sideBarItem in this.GetSectionsWithCoverAndComplete())
                this.sectionItems.Add(sideBarItem);

            this.AllVisibleSections = new ObservableRangeCollection<ISideBarItem>(this.sectionItems);
        }

        private void OnScreenChanged(ScreenChangedEventArgs e)
        {
            var expandedSectionIdentities = this.GetExpandedSectionsIdentities(e.TargetGroup).ToList();

            lock (sectionViewModelsLock)
            {
            foreach (var removedSectionViewModel in this.GetSectionsToRemove(expandedSectionIdentities).ToList())
                this.sectionItems.Remove(removedSectionViewModel);
            }

            foreach (var expandedGroup in expandedSectionIdentities)
                this.ExpandSection(expandedGroup, false);

            this.UpdateUI();
        }

        private IEnumerable<ISideBarItem> GetSectionsToRemove(List<Identity> expandedSectionIdentities)
        {
            var enabledSectionIdentites = this.GetEnabledSectionIdentites().ToList();

            foreach (var sectionViewModel in this.sectionViewModels)
            {
                if (enabledSectionIdentites.Contains(sectionViewModel.SectionIdentity))
                    continue;

                if (expandedSectionIdentities.Contains(sectionViewModel.SectionIdentity))
                    continue;
                
                if(expandedSectionIdentities.Contains(sectionViewModel.ParentIdentity))
                    continue;

                yield return sectionViewModel;
            }
        }

        private void OnSideBarSectionUpdated(SideBarSectionUpdateMessage e)
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            lock (sectionViewModelsLock)
            {

            var updatedGroupViewModel = this.sectionViewModels.FirstOrDefault(x => x.SectionIdentity == e.UpdatedGroup);
            if (updatedGroupViewModel == null) return;

            var indexOfUpdatedGroup = this.sectionItems.IndexOf(updatedGroupViewModel);

            var enabledSubSections = interview.GetEnabledSubgroups(e.UpdatedGroup).ToList();

            foreach (var sectionIdentity in enabledSubSections)
            {
                if (this.sectionViewModels.Any(section => section.SectionIdentity == sectionIdentity))
                    continue;

                var sectionViewModel = this.modelsFactory.BuildSectionItem(sectionIdentity,
                    this.navigationState, this.interviewId);

                var indexOfSubsection = indexOfUpdatedGroup + enabledSubSections.IndexOf(sectionIdentity) + 1;

                this.sectionItems.Insert(indexOfSubsection, sectionViewModel);

                }
            }

            this.UpdateUI();
        }

        private void OnSideBarSectionCollapsed(SideBarSectionCollapseMessage e)
            => this.CollapseSection(e.CollapsedGroup);

        private void OnSideBarSectionExpanded(SideBarSectionExpandMessage e) 
            => this.ExpandSection(e.ExpandedGroup);

        private void CollapseSection(Identity collapsedSection)
        {
            var collapsedViewModels = this.sectionViewModels.Where(
                sectionViewModel => sectionViewModel.ParentsIdentities.Contains(collapsedSection)).ToList();

            foreach (var collapsedViewModel in collapsedViewModels)
                this.sectionItems.Remove(collapsedViewModel);

            this.UpdateUI();
        }

        private void ExpandSection(Identity expandedSection, bool force = true)
        {
            ISideBarSectionItem expandedGroupViewModel;

            lock (sectionViewModelsLock)
            {
                expandedGroupViewModel = this.sectionViewModels
                .FirstOrDefault(section => section.SectionIdentity == expandedSection);

            if (expandedGroupViewModel == null) return;
            if (!force && expandedGroupViewModel.Expanded) return;

            var indexOfExpandedGroup = this.sectionItems.IndexOf(expandedGroupViewModel);

            foreach (var subSection in this.GetSubSections(expandedSection))
                this.sectionItems.Insert(indexOfExpandedGroup + 1, subSection);
            }

            this.UpdateUI();
        }

        private void AddSection(Identity addedSection)
        {
            var enabledSections = this.GetEnabledSectionIdentites().ToList();

            var indexOfSection = enabledSections.IndexOf(addedSection);
            lock (sectionViewModelsLock)
            {
            var prevSectionIdentity = enabledSections.ElementAt(indexOfSection - 1);
            var prevSectionViewModel = this.sectionViewModels.FirstOrDefault(viewModel => viewModel.SectionIdentity == prevSectionIdentity);

            if (prevSectionViewModel != null)
            {
                var indexOfPrevSection = this.sectionItems.IndexOf(prevSectionViewModel);
                var numberOfPrevSectionChildren = this.sectionViewModels.Count(
                    viewModel => viewModel.ParentsIdentities.Contains(prevSectionIdentity));

                indexOfSection = indexOfPrevSection + numberOfPrevSectionChildren + 1;
            }

            var sectionViewModel = this.modelsFactory.BuildSectionItem(addedSection, this.navigationState,
                this.interviewId);

            this.sectionItems.Insert(indexOfSection, sectionViewModel);
            }
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
            var addedSections = @event.Groups.Intersect(this.sectionIdentities).ToArray();
            foreach (var addedSection in addedSections)
                this.AddSection(addedSection);

            if(addedSections.Any())
                this.UpdateUI();
        }

        public void Handle(GroupsDisabled @event)
        {
            List<ISideBarSectionItem> removedViewModels;

            lock (sectionViewModelsLock)
            {
                removedViewModels = this.sectionViewModels.Where(
                    sectionViewModel => @event.Groups.Contains(sectionViewModel.SectionIdentity)).ToList();

            foreach (var removedViewModel in removedViewModels)
                this.sectionItems.Remove(removedViewModel);
            }
            if(removedViewModels.Any())
                this.UpdateUI();
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            List<ISideBarSectionItem> removedViewModels;

            var removedViewModels = this.sectionViewModels.Where(sectionViewModel =>
                    @event.Instances.Select(x => x.GetIdentity()).Contains(sectionViewModel.SectionIdentity)).ToList();

            foreach (var removedViewModel in removedViewModels)
                this.sectionItems.Remove(removedViewModel);

            if(removedViewModels.Any())
                this.UpdateUI();
        }
        
        private void UpdateUI()
        {
            this.InvokeOnMainThread(() => {
                var notChangedItems = this.AllVisibleSections.Intersect(this.sectionItems).ToArray();
                var removedViewModels = this.AllVisibleSections.Except(notChangedItems).ToArray();

                foreach (var removedViewModel in removedViewModels)
                    removedViewModel.Dispose();

                this.AllVisibleSections.RemoveRange(removedViewModels);

                lock (sectionViewModelsLock)
                {
                foreach (var addedSectionViewModels in this.sectionViewModels.Except(notChangedItems).OfType<ISideBarSectionItem>().GroupBy(x=>x.SectionIdentity.Id))
                    this.AllVisibleSections.InsertRange(this.sectionItems.IndexOf(addedSectionViewModels.First()), addedSectionViewModels);
                }
            });
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);

            this.messenger.Unsubscribe<SideBarSectionCollapseMessage>(this.sectionCollapseSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionExpandMessage>(this.sectionExpandSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionUpdateMessage>(this.sectionUpdateSubscriptionToken);

            this.AllVisibleSections?.ForEach(x => x.Dispose());
        }
    }
}
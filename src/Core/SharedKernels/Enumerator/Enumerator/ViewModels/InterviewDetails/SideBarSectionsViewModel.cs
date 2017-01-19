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
    public class SideBarSectionsViewModel : MvxNotifyPropertyChanged,
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

        public SynchronizedList<ISideBarItem> allVisibleSections = new SynchronizedList<ISideBarItem>();
        public ObservableRangeCollection<ISideBarItem> AllVisibleSections { get; private set; }

        private IEnumerable<ISideBarSectionItem> sectionViewModels => this.allVisibleSections.OfType<ISideBarSectionItem>();

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
                this.allVisibleSections.Add(sideBarItem);

            this.AllVisibleSections = new ObservableRangeCollection<ISideBarItem>(this.allVisibleSections);
            this.RaisePropertyChanged(() => this.AllVisibleSections);
        }

        private void OnScreenChanged(ScreenChangedEventArgs e)
        {
            var expandedSectionIdentities = this.GetExpandedSectionsIdentities(e.TargetGroup).ToList();

            foreach (var removedSectionViewModel in this.GetSectionsToRemove(expandedSectionIdentities).ToList())
                this.allVisibleSections.Remove(removedSectionViewModel);

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

            var updatedGroupViewModel = this.sectionViewModels.FirstOrDefault(x => x.SectionIdentity == e.UpdatedGroup);
            if (updatedGroupViewModel == null) return;

            var indexOfUpdatedGroup = this.allVisibleSections.IndexOf(updatedGroupViewModel);

            var enabledSubSections = interview.GetEnabledSubgroups(e.UpdatedGroup).ToList();

            foreach (var sectionIdentity in enabledSubSections)
            {
                if (this.sectionViewModels.Any(section => section.SectionIdentity == sectionIdentity))
                    continue;

                var sectionViewModel = this.modelsFactory.BuildSectionItem(sectionIdentity,
                    this.navigationState, this.interviewId);

                var indexOfSubsection = indexOfUpdatedGroup + enabledSubSections.IndexOf(sectionIdentity) + 1;

                this.allVisibleSections.Insert(indexOfSubsection, sectionViewModel);
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
                this.allVisibleSections.Remove(collapsedViewModel);

            this.UpdateUI();
        }

        private void ExpandSection(Identity expandedSection, bool force = true)
        {
            var expandedGroupViewModel = this.sectionViewModels
                .FirstOrDefault(section => section.SectionIdentity == expandedSection);

            if (expandedGroupViewModel == null) return;
            if (!force && expandedGroupViewModel.Expanded) return;

            var indexOfExpandedGroup = this.allVisibleSections.IndexOf(expandedGroupViewModel);

            foreach (var subSection in this.GetSubSections(expandedSection))
                this.allVisibleSections.Insert(indexOfExpandedGroup + 1, subSection);

            this.UpdateUI();
        }

        private void AddSection(Identity addedSection)
        {
            var enabledSections = this.GetEnabledSectionIdentites().ToList();

            var indexOfSection = enabledSections.IndexOf(addedSection);

            var prevSectionIdentity = enabledSections.ElementAt(indexOfSection - 1);
            var prevSectionViewModel = this.sectionViewModels.FirstOrDefault(viewModel => viewModel.SectionIdentity == prevSectionIdentity);

            if (prevSectionViewModel != null)
            {
                var indexOfPrevSection = this.allVisibleSections.IndexOf(prevSectionViewModel);
                var numberOfPrevSectionChildren = this.sectionViewModels.Count(
                    viewModel => viewModel.ParentsIdentities.Contains(prevSectionIdentity));

                indexOfSection = indexOfPrevSection + numberOfPrevSectionChildren + 1;
            }

            var sectionViewModel = this.modelsFactory.BuildSectionItem(addedSection, this.navigationState,
                this.interviewId);

            this.allVisibleSections.Insert(indexOfSection, sectionViewModel);
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
            var removedViewModels = this.sectionViewModels.Where(
                    sectionViewModel => @event.Groups.Contains(sectionViewModel.SectionIdentity)).ToList();

            foreach (var removedViewModel in removedViewModels)
                this.allVisibleSections.Remove(removedViewModel);

            if(removedViewModels.Any())
                this.UpdateUI();
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            var removedViewModels = this.sectionViewModels.Where(sectionViewModel =>
                    @event.Instances.Select(x => x.GetIdentity()).Contains(sectionViewModel.SectionIdentity)).ToList();

            foreach (var removedViewModel in removedViewModels)
                this.allVisibleSections.Remove(removedViewModel);

            if(removedViewModels.Any())
                this.UpdateUI();
        }
        
        private void UpdateUI()
        {
            this.InvokeOnMainThread(() => {
                var notChangedItems = this.AllVisibleSections.Intersect(this.allVisibleSections).ToArray();
                var removedViewModels = this.AllVisibleSections.Except(notChangedItems).ToArray();

                this.AllVisibleSections.RemoveRange(removedViewModels);

                foreach (var addedSectionViewModels in this.sectionViewModels.Except(notChangedItems).OfType<ISideBarSectionItem>().GroupBy(x=>x.SectionIdentity))
                    this.AllVisibleSections.InsertRange(this.allVisibleSections.IndexOf(addedSectionViewModels.First()), addedSectionViewModels);
            });
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);

            this.messenger.Unsubscribe<SideBarSectionCollapseMessage>(this.sectionCollapseSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionExpandMessage>(this.sectionExpandSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionUpdateMessage>(this.sectionUpdateSubscriptionToken);

            this.AllVisibleSections.ForEach(x => x.Dispose());
        }
    }
}
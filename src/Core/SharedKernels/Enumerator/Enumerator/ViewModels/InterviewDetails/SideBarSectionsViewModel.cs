using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SideBarSectionsViewModel : MvxNotifyPropertyChanged, IDisposable
    {
        private readonly IMvxMessenger messenger;
        private NavigationState navigationState;

        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ISideBarSectionViewModelsFactory modelsFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private QuestionnaireIdentity questionnaireIdentity;
        private string interviewId;
        private readonly MvxSubscriptionToken sectionExpandSubscriptionToken;
        private readonly MvxSubscriptionToken sectionCollapseSubscriptionToken;
        private readonly MvxSubscriptionToken sectionRemoveSubscriptionToken;
        private readonly MvxSubscriptionToken sectionUpdateSubscriptionToken;

        private ObservableCollection<ISideBarItem> allVisibleSections;
        public ObservableCollection<ISideBarItem> AllVisibleSections {
            get {return this.allVisibleSections;}
            set { this.RaiseAndSetIfChanged(ref this.allVisibleSections, value); }
        }

        private IEnumerable<ISideBarSectionItem> sectionViewModels => this.AllVisibleSections.OfType<ISideBarSectionItem>();

        public SideBarSectionsViewModel(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            ISideBarSectionViewModelsFactory modelsFactory,
            IMvxMessenger messenger)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.modelsFactory = modelsFactory;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.messenger = messenger;
            this.sectionExpandSubscriptionToken = this.messenger.Subscribe<SideBarSectionExpandMessage>(OnSideBarSectionExpanded);
            this.sectionCollapseSubscriptionToken = this.messenger.Subscribe<SideBarSectionCollapseMessage>(OnSideBarSectionCollapsed);
            this.sectionRemoveSubscriptionToken = this.messenger.Subscribe<SideBarSectionRemoveMessage>(OnSideBarSectionRemoved);
            this.sectionUpdateSubscriptionToken = this.messenger.Subscribe<SideBarSectionUpdateMessage>(OnSideBarSectionUpdated);
        }

        public void Init(string interviewId, QuestionnaireIdentity questionnaireId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException(nameof(navigationState));
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionnaireId == null) throw new ArgumentNullException(nameof(questionnaireId));

            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.interviewId = interviewId;
            this.navigationState = navigationState;
            this.questionnaireIdentity = questionnaireId;
            this.navigationState.ScreenChanged += this.OnScreenChanged;

            this.CreateCoverAndCompleteSections();
        }

        private void OnScreenChanged(ScreenChangedEventArgs e) => this.UpdateSectionsList(e.TargetGroup);

        private void UpdateSectionsList(Identity expandedGroupIdentity = null)
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.questionnaireIdentity, interview.Language);

            var enabledSections = questionnaire.GetAllSections()
                .Select(sectionId => Identity.Create(sectionId, RosterVector.Empty))
                .Where(interview.IsEnabled)
                .ToList();

            var allSubSections = this.sectionViewModels.Where(
                sectionViewModel => !enabledSections.Contains(sectionViewModel.SectionIdentity)).ToList();
            foreach (var subSection in allSubSections)
            {
                this.DisposeAndRemoveSectionViewModel(subSection);
            }

            foreach (var sectionIdentity in enabledSections)
            {
                if (this.sectionViewModels.Any(section => section.SectionIdentity == sectionIdentity))
                    continue;

                var sectionViewModel = this.modelsFactory.BuildSectionItem(sectionIdentity,
                    this.navigationState, this.interviewId);

                this.AllVisibleSections.Insert(enabledSections.IndexOf(sectionIdentity) + 1, sectionViewModel);
            }

            if (expandedGroupIdentity == null) return;

            var expandedSections = interview.GetGroup(expandedGroupIdentity)
                .Parents?.Select(parentGroup => parentGroup.Identity) ?? Enumerable.Empty<Identity>();

            foreach (var expandedGroup in expandedSections.Concat(new[] {expandedGroupIdentity}))
                this.ExpandSection(expandedGroup);
        }

        private void CreateCoverAndCompleteSections()
        {
            this.AllVisibleSections = new ObservableCollection<ISideBarItem>
            {
                this.modelsFactory.BuildCoverItem(this.navigationState),
                this.modelsFactory.BuildCompleteItem(this.navigationState, this.interviewId)
            };
        }

        private void OnSideBarSectionUpdated(SideBarSectionUpdateMessage e)
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            var updatedGroupViewModel = this.sectionViewModels.FirstOrDefault(x => x.SectionIdentity == e.UpdatedGroup);
            if (updatedGroupViewModel == null) return;

            var indexOfUpdatedGroup = this.AllVisibleSections.IndexOf(updatedGroupViewModel);

            var enabledSubSections = interview.GetEnabledSubgroups(e.UpdatedGroup).ToList();

            this.InvokeOnMainThread(() =>
                {
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
            );
        }

        private void OnSideBarSectionRemoved(SideBarSectionRemoveMessage e)
            => this.DisposeAndRemoveSection(e.RemovedGroup);

        private void OnSideBarSectionCollapsed(SideBarSectionCollapseMessage e)
        {
            var collapsedViewModels = this.sectionViewModels.Where(
                    sectionViewModel => sectionViewModel.ParentsIdentities.Contains(e.CollapsedGroup)).ToList();
            foreach (var collapsedViewModel in collapsedViewModels)
            {
                this.DisposeAndRemoveSectionViewModel(collapsedViewModel);
            }
        }

        private void OnSideBarSectionExpanded(SideBarSectionExpandMessage e) => this.ExpandSection(e.ExpandedGroup);

        private void ExpandSection(Identity expandedSection)
        {
            var expandedGroupViewModel = this.sectionViewModels
                .FirstOrDefault(section => section.SectionIdentity == expandedSection);

            if (expandedGroupViewModel == null) return;

            var indexOfExpandedGroup = this.AllVisibleSections.IndexOf(expandedGroupViewModel);

            foreach (var subSection in this.GetSubSections(expandedSection))
            {
                this.AllVisibleSections.Insert(++indexOfExpandedGroup, subSection);
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

        private void DisposeAndRemoveSection(Identity sectionIdentity)
        {
            var sectionWithChildrenViewModels = this.sectionViewModels
                .Where(section =>
                    section.SectionIdentity == sectionIdentity || section.ParentsIdentities.Contains(sectionIdentity))
                .ToList();


            foreach (var sectionWithChildrenViewModel in sectionWithChildrenViewModels)
            {
                this.DisposeAndRemoveSectionViewModel(sectionWithChildrenViewModel);
            }
        }

        private void DisposeAndRemoveSectionViewModel(ISideBarSectionItem section)
        {
            section.Dispose();
            this.AllVisibleSections.Remove(section);
        }

        public void Dispose()
        {
            this.messenger.Unsubscribe<SideBarSectionCollapseMessage>(this.sectionCollapseSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionExpandMessage>(this.sectionExpandSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionRemoveMessage>(this.sectionRemoveSubscriptionToken);
            this.messenger.Unsubscribe<SideBarSectionUpdateMessage>(this.sectionUpdateSubscriptionToken);

            this.AllVisibleSections.ForEach(x => x.Dispose());
        }
    }
}
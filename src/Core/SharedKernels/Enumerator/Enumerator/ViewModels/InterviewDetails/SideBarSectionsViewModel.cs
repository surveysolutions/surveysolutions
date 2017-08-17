using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
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
        private readonly ILiteEventRegistry eventRegistry;
        private NavigationState navigationState;

        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ISideBarSectionViewModelsFactory modelsFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private string interviewId;
        private List<Identity> sectionIdentities;

        public SynchronizedList<ISideBarSectionItem> items = new SynchronizedList<ISideBarSectionItem>();

        private ObservableRangeCollection<ISideBarItem> allVisibleSections;
        public ObservableRangeCollection<ISideBarItem> AllVisibleSections
        {
            get { return this.allVisibleSections; }
            set { this.RaiseAndSetIfChanged(ref this.allVisibleSections, value); }
        }

        public SideBarSectionsViewModel(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            ISideBarSectionViewModelsFactory modelsFactory,
            ILiteEventRegistry eventRegistry)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.modelsFactory = modelsFactory;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.eventRegistry = eventRegistry;
        }

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException(nameof(navigationState));
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));

            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.interviewId = interviewId;
            this.navigationState = navigationState;
            this.navigationState.ScreenChanged += this.OnScreenChanged;

            this.eventRegistry.Subscribe(this, interviewId);

            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.sectionIdentities = questionnaire.GetAllSections()
                .Select(sectionId => Identity.Create(sectionId, RosterVector.Empty))
                .ToList();

            this.AllVisibleSections = new ObservableRangeCollection<ISideBarItem>(new[]
            {
                this.modelsFactory.BuildCoverItem(this.navigationState),
                this.modelsFactory.BuildCompleteItem(this.navigationState, this.interviewId)
            });

            this.UpdateSections();
        }

        private void OnScreenChanged(ScreenChangedEventArgs e) => this.UpdateSections(true);

        public void Handle(GroupsEnabled @event)
        {
            var addedSections = @event.Groups.Intersect(this.sectionIdentities).ToArray();
            
            if (addedSections.Any())
                this.UpdateSections();
        }

        public void Handle(GroupsDisabled @event)
        {
            var removedViewModels = this.items.Where(
                    sectionViewModel => @event.Groups.Contains(sectionViewModel.SectionIdentity)).ToList();

            if (removedViewModels.Any())
                this.UpdateSections();
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            var removedViewModels = this.items.Where(sectionViewModel =>
                    @event.Instances.Select(x => x.GetIdentity()).Contains(sectionViewModel.SectionIdentity)).ToList();

            if (removedViewModels.Any())
                this.UpdateSections();
        }

        private void UpdateSections(object sender, EventArgs e) => this.UpdateSections();
        private void UpdateSections(bool force = false)
        {
            this.UpdateViewModels(force);
            this.UpdateUI();
        }


        private void UpdateViewModels(bool force)
        {
            var expectedSectionIdentities = this.GetSectionsAndExpandedSubSections(force).ToList();
            var viewModelIdentities = this.items.Select(viewModel => viewModel.SectionIdentity).ToArray();

            var notChangedSectionIdentities = expectedSectionIdentities.Intersect(viewModelIdentities).ToArray();

            var removedViewModelIdentities = viewModelIdentities.Except(notChangedSectionIdentities).ToArray();
            var addedViewModelIdentities = expectedSectionIdentities.Except(notChangedSectionIdentities).ToArray();

            foreach (var identity in removedViewModelIdentities)
            {
                var removedViewModel = this.items.FirstOrDefault(viewModel => viewModel.SectionIdentity == identity);
                if (removedViewModel == null) continue;

                this.DisposeSectionViewModel(removedViewModel);
                this.items.Remove(removedViewModel);
            }

            foreach (var identity in addedViewModelIdentities)
            {
                if (this.isDisposed) return;
                this.items.Insert(expectedSectionIdentities.IndexOf(identity), this.CreateSideBarSectionItem(identity));
            }
        }

        private void UpdateUI() => this.InvokeOnMainThread(() =>
        {
            var sectionItems = this.AllVisibleSections.OfType<ISideBarSectionItem>().ToArray();

            var notChangedItems = sectionItems.Intersect(this.items).ToArray();
            var removedViewModels = sectionItems.Except(notChangedItems).ToArray();
            var addedSectionViewModels = this.items.Except(notChangedItems);

            this.AllVisibleSections.RemoveRange(removedViewModels);

            foreach (var addedSectionViewModel in addedSectionViewModels)
                this.AllVisibleSections.Insert(this.items.IndexOf(addedSectionViewModel) + 1, addedSectionViewModel);
        });

        private IEnumerable<Identity> GetSectionsAndExpandedSubSections(bool clearExpanded)
        {
            var expandedSectionIdentities = this.items
                .Where(section => section.Expanded)
                .Select(expandedSection => expandedSection.SectionIdentity)
                .ToArray();

            var interview = this.statefulInterviewRepository.Get(this.interviewId);

            var currentGroup = interview.GetGroup(this.navigationState.CurrentGroup);
            var parentsOfCurrentGroup = currentGroup?.Parents?.Select(group => group.Identity).ToArray() ?? new Identity[0];

            foreach (var sectionOrSubSection in interview.GetAllEnabledGroupsAndRosters())
            {
                if (sectionOrSubSection is InterviewTreeSection)
                    yield return sectionOrSubSection.Identity;

                if(sectionOrSubSection.Parent == null) continue;

                var isInCurrentSection = sectionOrSubSection.Parent.Identity == currentGroup?.Identity;
                var isParentOfCurrentSection = parentsOfCurrentGroup.Contains(sectionOrSubSection.Parent.Identity);
                var isExpandedSection = expandedSectionIdentities.Contains(sectionOrSubSection.Parent.Identity);

                if (clearExpanded && (isParentOfCurrentSection || isInCurrentSection))
                    yield return sectionOrSubSection.Identity;

                if (!clearExpanded && isExpandedSection)
                    yield return sectionOrSubSection.Identity;
            }
        }

        private ISideBarSectionItem CreateSideBarSectionItem(Identity sectionIdentity)
        {
            var sectionViewModel = this.modelsFactory.BuildSectionItem(sectionIdentity, this.navigationState,
                this.interviewId);

            sectionViewModel.OnSectionUpdated += this.UpdateSections;
            return sectionViewModel;
        }

        private bool isDisposed;
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;

                this.eventRegistry.Unsubscribe(this);
                this.AllVisibleSections?.ForEach(viewModel =>
                {
                    var sectionViewModel = viewModel as ISideBarSectionItem;
                    if (sectionViewModel != null)
                        this.DisposeSectionViewModel(sectionViewModel);
                    else
                        viewModel.Dispose();
                });
            }
        }

        private void DisposeSectionViewModel(ISideBarSectionItem viewModel)
        {
            viewModel.OnSectionUpdated -= this.UpdateSections;
            viewModel.Dispose();
        }
    }
}
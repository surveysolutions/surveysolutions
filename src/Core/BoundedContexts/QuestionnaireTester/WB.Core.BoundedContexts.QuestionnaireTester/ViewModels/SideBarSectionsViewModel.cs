using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SideBarSectionsViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>,
        ILiteEventHandler<RosterInstancesTitleChanged>
    {
        private NavigationState navigationState;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private readonly IMvxMessenger messenger;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private string questionnaireId;
        private string interviewId;

        public IList<SideBarSectionViewModel> Sections { get; set; }

        public SideBarSectionsViewModel(
            IMvxMessenger messenger,
            IStatefulInterviewRepository statefulInterviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.substitutionService = substitutionService;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.messenger = messenger;
            this.statefulInterviewRepository = statefulInterviewRepository;
            eventRegistry.Subscribe(this);
        }

        public void Init(string questionnaireId,
            string interviewId,
            NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.interviewId = interviewId;
            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;
            this.questionnaireId = questionnaireId;

            BuildSectionsList();
        }

        private void BuildSectionsList()
        {
            var questionnaire = this.questionnaireRepository.GetById(questionnaireId);
            var interview = this.statefulInterviewRepository.Get(interviewId);

            List<SideBarSectionViewModel> sections = new List<SideBarSectionViewModel>();

            foreach (GroupsHierarchyModel section in questionnaire.GroupsHierarchy)
            {
                var sectionViewModel = this.BuildSectionViewModel(this, section);
                foreach (var child in section.Children)
                {
                    this.BuildViewModelsForSection(interview, sectionViewModel, child);
                }

                sections.Add(sectionViewModel);
            }

            this.Sections = sections;
        }

        private SideBarSectionViewModel BuildSectionViewModel(SideBarSectionsViewModel root, GroupsHierarchyModel section)
        {
            return new SideBarSectionViewModel(root, null, 0)
            {
                Title = section.Title,
                SectionIdentity = new Identity(section.Id, new decimal[] { })
            };
        }

        private void BuildViewModelsForSection(IStatefulInterview statefulInterview, SideBarSectionViewModel parent, GroupsHierarchyModel @group)
        {
            IEnumerable<Identity> groupInstances = Enumerable.Empty<Identity>(); ;
            if (group.IsRoster)
            {
                groupInstances = statefulInterview.GetEnabledGroupInstances(group.Id, parent.SectionIdentity.RosterVector);
            }
            else
            {
                var identity = new Identity(@group.Id, parent.SectionIdentity.RosterVector);
                if (statefulInterview.IsEnabled(identity))
                {
                    groupInstances = identity.ToEnumerable();
                }
            }

            foreach (Identity groupInstance in groupInstances)
            {
                string title = group.Title;
                if (group.IsRoster)
                {
                    var rosterTitle = statefulInterview.GetRosterTitle(groupInstance);
                    title = substitutionService.GenerateRosterName(group.Title, rosterTitle);
                }

                var section = new SideBarSectionViewModel(this, parent, group.ZeroBasedDepth)
                {
                    SectionIdentity = groupInstance,
                    Title = title
                };

                this.mainThreadDispatcher.RequestMainThreadAction(() => parent.Children.Add(section));

                foreach (var child in group.Children)
                {
                    this.BuildViewModelsForSection(statefulInterview, section, child);
                }
            }
        }

        internal async Task NavigateToSection(SideBarSectionViewModel item)
        {
            messenger.Publish(new SectionChangeMessage(this));

            if (item.IsCurrent)
            {
                return;
            }

            await this.navigationState.NavigateTo(item.SectionIdentity);
        }

        void navigationState_OnGroupChanged(NavigationParams navigationParams)
        {
            HighlightCurrentGroup(navigationParams);
            HighlightCurrentSection(navigationParams);
        }

        private void HighlightCurrentGroup(NavigationParams navigationParams)
        {
            var oldSelectedGroups = this.Sections.TreeToEnumerable(x => x.Children)
                .Where(x => x.IsCurrent || x.Expanded);
            oldSelectedGroups.ForEach(x => x.IsCurrent = false);

            SideBarSectionViewModel newCurrentGroup = this.Sections.TreeToEnumerable(x => x.Children)
                .FirstOrDefault(x => x.SectionIdentity.Equals(navigationParams.TargetGroup));

            newCurrentGroup.UnwrapReferences(x => x.Parent).ForEach(x => x.Expanded = true);
            newCurrentGroup.IsCurrent = true;
        }

        private void HighlightCurrentSection(NavigationParams navigationParams)
        {
            SideBarSectionViewModel sideBarSectionToHighlight = null;
            foreach (var section in this.Sections)
            {
                SideBarSectionViewModel selectedGroup = section.TreeToEnumerable(x => x.Children)
                    .FirstOrDefault(x => x.SectionIdentity.Equals(navigationParams.TargetGroup));

                if (selectedGroup != null)
                {
                    sideBarSectionToHighlight = section;
                    break;
                }
            }

            if (sideBarSectionToHighlight == null)
            {
                return;
            }

            this.Sections.Where(x => x.IsSelected).ForEach(x => x.IsSelected = false);
            sideBarSectionToHighlight.IsSelected = true;
        }

        public void Handle(RosterInstancesAdded @event)
        {
            var groupsToUpdate = @event.Instances.Select(x => x.GroupId).Distinct().ToList();
            foreach (var groupId in groupsToUpdate)
            {
                this.UpdateParentOfGroupWithId(groupId);
            }
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            var groupsToUpdate = @event.Instances.Select(x => x.GroupId).Distinct();
            foreach (var groupId in groupsToUpdate)
            {
                this.UpdateParentOfGroupWithId(groupId);
            }
        }

        public void Handle(GroupsEnabled @event)
        {
            var groupsToUpdate = @event.Groups.Select(x => x.Id).Distinct();
            foreach (var groupId in groupsToUpdate)
            {
                this.UpdateParentOfGroupWithId(groupId);
            }
        }

        public void Handle(GroupsDisabled @event)
        {
            var groupsToUpdate = @event.Groups.Select(x => x.Id).Distinct();
            foreach (var groupId in groupsToUpdate)
            {
                this.UpdateParentOfGroupWithId(groupId);
            }
        }

        private void UpdateParentOfGroupWithId(Guid groupId)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(this.interviewId);
            QuestionnaireModel questionnaire = this.questionnaireRepository.GetById(this.questionnaireId);

            if (questionnaire.GroupsParentIdMap.ContainsKey(groupId))
            {
                Guid? parentGroupId = questionnaire.GroupsParentIdMap[groupId];
                SideBarSectionViewModel viewModelToUpdate = this.Sections.TreeToEnumerable(x => x.Children)
                                                                  .FirstOrDefault(x => x.SectionIdentity.Id == parentGroupId);
                if (viewModelToUpdate != null)
                {
                    this.mainThreadDispatcher.RequestMainThreadAction(() => viewModelToUpdate.Children.Clear());

                    GroupsHierarchyModel groupToAddTo = questionnaire.GroupsHierarchy
                                                                     .TreeToEnumerable(x => x.Children)
                                                                     .First(x => x.Id == parentGroupId);

                    foreach (var childGroup in groupToAddTo.Children)
                    {
                        this.BuildViewModelsForSection(interview, viewModelToUpdate, childGroup);
                    }
                }
            }
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var affectedRosterIdentities = @event.ChangedInstances.Select(x => x.RosterInstance.GetIdentity()).ToList();
            var affectedViewModels = this.Sections.TreeToEnumerable(x => x.Children)
                                         .Where(x => affectedRosterIdentities.Any(i => i.Equals(x.SectionIdentity)));

            QuestionnaireModel questionnaire = this.questionnaireRepository.GetById(this.questionnaireId);

            foreach (var affectedViewModel in affectedViewModels)
            {
                string groupTitle = questionnaire.GroupsWithFirstLevelChildrenAsReferences[affectedViewModel.SectionIdentity.Id].Title;
                string rosterTitle = @event.ChangedInstances.Single(x => x.RosterInstance.GetIdentity().Equals(affectedViewModel.SectionIdentity)).Title;

                string sectionFullName = this.substitutionService.GenerateRosterName(groupTitle, rosterTitle);
                affectedViewModel.Title = sectionFullName;
            }
        }
    }
}
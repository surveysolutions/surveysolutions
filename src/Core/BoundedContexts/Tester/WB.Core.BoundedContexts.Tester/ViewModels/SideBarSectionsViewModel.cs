using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class SideBarSectionsViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<GroupsEnabled>
    {
        private NavigationState navigationState;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        readonly ILiteEventRegistry eventRegistry;
        private readonly ISideBarSectionViewModelsFactory modelsFactory;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private string questionnaireId;
        private string interviewId;

        public IList<SideBarSectionViewModel> Sections { get; set; }

        public SideBarSectionsViewModel(IStatefulInterviewRepository statefulInterviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            ILiteEventRegistry eventRegistry,
            ISideBarSectionViewModelsFactory modelsFactory,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.eventRegistry = eventRegistry;
            this.modelsFactory = modelsFactory;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.statefulInterviewRepository = statefulInterviewRepository;
        }

        public void Init(string questionnaireId,
            string interviewId,
            NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questionnaireId == null) throw new ArgumentNullException("questionnaireId");

            eventRegistry.Subscribe(this, interviewId);

            this.navigationState = navigationState;
            this.navigationState.GroupChanged += this.NavigationStateGroupChanged;
            this.questionnaireId = questionnaireId;
            this.interviewId = interviewId;

            BuildSectionsList();
        }

        private void BuildSectionsList()
        {
            var questionnaire = this.questionnaireRepository.GetById(questionnaireId);
            List<SideBarSectionViewModel> sections = new List<SideBarSectionViewModel>();
            foreach (GroupsHierarchyModel section in questionnaire.GroupsHierarchy)
            {
                var groupIdentity = new Identity(section.Id, new decimal[] { });
                var sectionViewModel = this.BuildSectionItem(null, groupIdentity);

                sections.Add(sectionViewModel);
            }

            this.Sections = sections;
        }

        void NavigationStateGroupChanged(GroupChangedEventArgs navigationParams)
        {
            HighlightCurrentSection(navigationParams);
        }

        private void HighlightCurrentSection(GroupChangedEventArgs navigationParams)
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

            this.Sections.Where(x => x != sideBarSectionToHighlight && x.IsSelected).ForEach(x => x.IsSelected = false);
            this.Sections.Where(x => x != sideBarSectionToHighlight && x.Expanded).ForEach(x => x.Expanded = false);

            sideBarSectionToHighlight.IsSelected = true;
            sideBarSectionToHighlight.Expanded = true;
        }

        public void Handle(RosterInstancesAdded @event)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(this.interviewId);

            foreach (var rosterInstance in @event.Instances)
            {
                var addedIdentity = rosterInstance.GetIdentity();
                this.RefreshListWithNewItemAdded(addedIdentity, interview);
            }
        }

        public void Handle(GroupsEnabled @event)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(this.interviewId);

            foreach (var groupId in @event.Groups)
            {
                var addedIdentity = new Identity(groupId.Id, groupId.RosterVector);
                this.RefreshListWithNewItemAdded(addedIdentity, interview);
            }
        }

        private void RefreshListWithNewItemAdded(Identity addedIdentity, IStatefulInterview interview)
        {
            Identity parentId = interview.GetParentGroup(addedIdentity);
            var allVisibleSections = this.Sections.TreeToEnumerable(x => x.Children);
            var sectionToAddTo = allVisibleSections.SingleOrDefault(x => x.SectionIdentity.Equals(parentId));

            if (sectionToAddTo != null)
            {
                List<Identity> enabledSubgroups = interview.GetEnabledSubgroups(parentId).ToList();
                this.mainThreadDispatcher.RequestMainThreadAction(() =>
                {
                    for (int i = 0; i < enabledSubgroups.Count; i++)
                    {
                        var enabledSubgroupIdentity = enabledSubgroups[i];
                        if (i >= sectionToAddTo.Children.Count || !sectionToAddTo.Children[i].SectionIdentity.Equals(enabledSubgroupIdentity))
                        {
                            var sideBarItem = this.BuildSectionItem(sectionToAddTo, enabledSubgroupIdentity);
                            if (i < sectionToAddTo.Children.Count)
                            {
                                sectionToAddTo.Children.Insert(i, sideBarItem);
                            }
                            else
                            {
                                sectionToAddTo.Children.Add(sideBarItem);
                            }
                        }
                    }
                });
            }

            foreach (var section in allVisibleSections)
            {
                section.RefreshHasChildrenFlag();
            }
        }

        private SideBarSectionViewModel BuildSectionItem(SideBarSectionViewModel sectionToAddTo, Identity enabledSubgroupIdentity)
        {
            return this.modelsFactory.BuildSectionItem(sectionToAddTo, enabledSubgroupIdentity, this.navigationState, this.interviewId);
        }
    }
}
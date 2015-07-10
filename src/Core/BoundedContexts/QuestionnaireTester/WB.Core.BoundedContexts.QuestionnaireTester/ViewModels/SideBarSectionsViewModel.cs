using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SideBarSectionsViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<GroupsEnabled>
    {
        private NavigationState navigationState;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private readonly IMvxMessenger messenger;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private string questionnaireId;
        private string interviewId;

        public IList<SideBarSectionViewModel> Sections { get; set; }

        public SideBarSectionsViewModel(IStatefulInterviewRepository statefulInterviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher,
            IMvxMessenger messenger)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.substitutionService = substitutionService;
            this.eventRegistry = eventRegistry;
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

            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;
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
                var sectionViewModel = this.BuildSectionViewModel(section);
                sections.Add(sectionViewModel);
            }

            this.Sections = sections;
        }

        private SideBarSectionViewModel BuildSectionViewModel(GroupsHierarchyModel section)
        {
            var vm = this.NewSideBarSectionViewModel();
            vm.Init(this.navigationState);
            vm.Title = section.Title;
            vm.SectionIdentity = new Identity(section.Id, new decimal[] { });
            vm.HasChildren = section.Children.Count > 0;

            return vm;
        }

        private SideBarSectionViewModel NewSideBarSectionViewModel()
        {
            var vm = new SideBarSectionViewModel(this.statefulInterviewRepository,
                this.questionnaireRepository,
                this.substitutionService,
                this.eventRegistry,
                this.mainThreadDispatcher,
                this.messenger);
            return vm;
        }

        void navigationState_OnGroupChanged(GroupChangedEventArgs navigationParams)
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

            this.Sections.Where(x => x.IsSelected).ForEach(x => x.IsSelected = false);
            sideBarSectionToHighlight.IsSelected = true;
        }

        public void Handle(RosterInstancesAdded @event)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(this.interviewId);
            QuestionnaireModel questionnaireModel = this.questionnaireRepository.GetById(this.questionnaireId);

            foreach (var rosterInstance in @event.Instances)
            {
                var addedIdentity = rosterInstance.GetIdentity();
                Identity parentId = interview.GetParentGroup(addedIdentity);
                var sectionToAddTo = this.Sections.TreeToEnumerable(x => x.Children)
                    .SingleOrDefault(x => x.SectionIdentity.Equals(parentId));

                List<Identity> enabledSubgroups = interview.GetEnabledSubgroups(parentId).ToList();
                mainThreadDispatcher.RequestMainThreadAction(() =>
                {
                    for (int i = 0; i < enabledSubgroups.Count; i++)
                    {
                        var enabledSubgroupIdentity = enabledSubgroups[i];
                        var model = questionnaireModel.GroupsWithFirstLevelChildrenAsReferences[enabledSubgroupIdentity.Id];
                        if (i < sectionToAddTo.Children.Count)
                        {
                            if (!sectionToAddTo.Children[i].SectionIdentity.Equals(enabledSubgroupIdentity))
                            {
                                var sideBarItem = this.BuildSectionItem(model, enabledSubgroupIdentity, interview);
                                sectionToAddTo.Children.Insert(i, sideBarItem);
                            }
                        }
                        else
                        {
                            var sideBarItem = this.BuildSectionItem(model, enabledSubgroupIdentity, interview);
                            sectionToAddTo.Children.Insert(i, sideBarItem);
                        }
                    }
                });
            }
        }

        private SideBarSectionViewModel BuildSectionItem(GroupModel model, Identity enabledSubgroupIdentity,
            IStatefulInterview interview)
        {
            var sideBarItem = this.NewSideBarSectionViewModel();
            sideBarItem.Init(this.navigationState);

            sideBarItem.Title = model.Title;
            sideBarItem.SectionIdentity = enabledSubgroupIdentity;
            sideBarItem.HasChildren = interview.GetEnabledSubgroups(enabledSubgroupIdentity).Any();
            if (model is RosterModel)
            {
                string rosterTitle = interview.GetRosterTitle(enabledSubgroupIdentity);
                sideBarItem.Title = this.substitutionService.GenerateRosterName(model.Title, rosterTitle);
            }
            return sideBarItem;
        }

        public void Handle(GroupsEnabled @event)
        {
            foreach (var groupId in @event.Groups)
            {
                //this.UpdateParentOfGroupWithId(groupId.Id, groupId.RosterVector.WithoutLast().ToArray());
            }
        }
    }
}
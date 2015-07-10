using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore.Core;
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
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private string questionnaireId;
        private string interviewId;

        public IList<SideBarSectionViewModel> Sections { get; set; }

        public SideBarSectionsViewModel(IStatefulInterviewRepository statefulInterviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.substitutionService = substitutionService;
            this.mainThreadDispatcher = mainThreadDispatcher;
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
            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            List<SideBarSectionViewModel> sections = new List<SideBarSectionViewModel>();

            foreach (GroupsHierarchyModel section in questionnaire.GroupsHierarchy)
            {
                var groupModel = questionnaire.GroupsWithFirstLevelChildrenAsReferences[section.Id];
                var groupIdentity = new Identity(section.Id, new decimal[] { });
                var sectionViewModel = this.BuildSectionItem(null, groupModel,groupIdentity, interview);
                sections.Add(sectionViewModel);
            }

            this.Sections = sections;
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
                this.RefreshListWithNewItemAdded(addedIdentity, interview, questionnaireModel);
            }
        }

        public void Handle(GroupsEnabled @event)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(this.interviewId);
            QuestionnaireModel questionnaireModel = this.questionnaireRepository.GetById(this.questionnaireId);

            foreach (var groupId in @event.Groups)
            {
                var addedIdentity = new Identity(groupId.Id, groupId.RosterVector);
                this.RefreshListWithNewItemAdded(addedIdentity, interview, questionnaireModel);
            }
        }

        private void RefreshListWithNewItemAdded(Identity addedIdentity, IStatefulInterview interview,
            QuestionnaireModel questionnaireModel)
        {
            Identity parentId = interview.GetParentGroup(addedIdentity);
            var sectionToAddTo = this.Sections.TreeToEnumerable(x => x.Children)
                .SingleOrDefault(x => x.SectionIdentity.Equals(parentId));

            List<Identity> enabledSubgroups = interview.GetEnabledSubgroups(parentId).ToList();
            this.mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                for (int i = 0; i < enabledSubgroups.Count; i++)
                {
                    var enabledSubgroupIdentity = enabledSubgroups[i];
                    var model = questionnaireModel.GroupsWithFirstLevelChildrenAsReferences[enabledSubgroupIdentity.Id];
                    if (i >= sectionToAddTo.Children.Count || !sectionToAddTo.Children[i].SectionIdentity.Equals(enabledSubgroupIdentity))
                    {
                        var sideBarItem = this.BuildSectionItem(sectionToAddTo, model, enabledSubgroupIdentity, interview);
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

        private SideBarSectionViewModel BuildSectionItem(SideBarSectionViewModel sectionToAddTo, GroupModel model, Identity enabledSubgroupIdentity, IStatefulInterview interview)
        {
            return SideBarSectionViewModel.BuildSectionItem(sectionToAddTo, model, enabledSubgroupIdentity, interview, substitutionService, navigationState);
        }
    }
}
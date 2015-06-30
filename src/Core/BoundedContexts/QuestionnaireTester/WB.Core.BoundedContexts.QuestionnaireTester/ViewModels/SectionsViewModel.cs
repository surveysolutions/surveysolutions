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
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SectionsViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesAdded>
    {
        private NavigationState navigationState;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private readonly IMvxMessenger messenger;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private string questionnaireId;
        private string interviewId;

        public IList<SectionViewModel> Sections { get; set; }

        public SectionsViewModel(
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

            List<SectionViewModel> sections = new List<SectionViewModel>();

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

        private SectionViewModel BuildSectionViewModel(SectionsViewModel root, GroupsHierarchyModel section)
        {
            return new SectionViewModel(root)
            {
                Title = section.Title,
                SectionIdentity = new Identity(section.Id, new decimal[] { })
            };
        }

        private void BuildViewModelsForSection(IStatefulInterview statefulInterview, SectionViewModel parent, GroupsHierarchyModel @group)
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

            foreach (var groupInstance in groupInstances)
            {
                string title = group.Title;
                if (group.IsRoster)
                {
                    var rosterTitle = statefulInterview.GetRosterTitle(groupInstance);
                    title = substitutionService.GenerateRosterName(group.Title, rosterTitle);
                }

                var section = new SectionViewModel(this)
                {
                    SectionIdentity = groupInstance,
                    Title = title
                };

                mainThreadDispatcher.RequestMainThreadAction(() => parent.Children.Add(section));

                foreach (var child in group.Children)
                {
                    this.BuildViewModelsForSection(statefulInterview, section, child);
                }
            }
        }

        internal async Task NavigateToSection(SectionViewModel item)
        {
            messenger.Publish(new SectionChangeMessage(this));

            if (item.IsSelected)
            {
                return;
            }

            await this.navigationState.NavigateTo(item.SectionIdentity);
        }

        void navigationState_OnGroupChanged(NavigationParams navigationParams)
        {
            var sectionToBeSelected = Sections.FirstOrDefault(x => x.SectionIdentity.Equals(navigationParams.TargetGroup));
            if (sectionToBeSelected == null)
            {
                return;
            }

            this.Sections.Where(x => x.IsSelected).ForEach(x => x.IsSelected = false);
            sectionToBeSelected.IsSelected = true;
        }

        public void Handle(RosterInstancesAdded @event)
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetById(this.questionnaireId);

            var instance = @event.Instances.First();
            if (questionnaire.GroupsParentIdMap.ContainsKey(instance.GroupId))
            {
                var parentGroupId = questionnaire.GroupsParentIdMap[instance.GroupId];
                var sectionToAddTo = this.Sections.FirstOrDefault(x => x.SectionIdentity.Id == parentGroupId);
                if (sectionToAddTo != null)
                {
                    mainThreadDispatcher.RequestMainThreadAction(() => sectionToAddTo.Children.Clear());

                    var groupToAddTo = questionnaire.GroupsHierarchy
                        .TreeToEnumerable(x => x.Children)
                        .First(x => x.Id == sectionToAddTo.SectionIdentity.Id);

                    foreach (var childGroup in groupToAddTo.Children)
                    {
                        BuildViewModelsForSection(interview, sectionToAddTo, childGroup);
                    }
                }
            }
        }
    }
}
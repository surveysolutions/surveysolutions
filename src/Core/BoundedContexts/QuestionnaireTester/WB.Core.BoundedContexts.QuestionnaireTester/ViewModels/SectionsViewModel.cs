using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SectionsViewModel : MvxNotifyPropertyChanged
    {
        private NavigationState navigationState;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IMvxMessenger messenger;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private string questionnaireId;
        private string interviewId;

        public IList<SectionViewModel> Sections { get; set; }

        public SectionsViewModel(
            IMvxMessenger messenger,
            IStatefulInterviewRepository statefulInterviewRepository,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            ISubstitutionService substitutionService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.substitutionService = substitutionService;
            this.messenger = messenger;
            this.statefulInterviewRepository = statefulInterviewRepository;
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
                    this.ProcessSection(interview, sectionViewModel, child);
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
                SectionIdentity = new Identity(section.Id, new decimal[]{})
            };
        }

        private void ProcessSection(IStatefulInterview statefulInterview, SectionViewModel parent, GroupsHierarchyModel @group)
        {
            IEnumerable<Identity> groupInstances;
            if (group.IsRoster)
            {
                groupInstances = statefulInterview.GetGroupInstances(group.Id, parent.SectionIdentity.RosterVector);

            }
            else
            {
                groupInstances = new Identity(group.Id, parent.SectionIdentity.RosterVector).ToEnumerable();
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
                parent.Children.Add(section);

                foreach (var child in group.Children)
                {
                    this.ProcessSection(statefulInterview, section, child);
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
    }
}
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class GroupNavigationViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        public class GroupStatistics
        {
            public int EnabledQuestionsCount { get; set; }
            public int SubgroupsCount { get; set; }
            public int AnsweredQuestionsCount { get; set; }
            public int UnansweredQuestionsCount { get; set; }
            public int InvalidAnswersCount { get; set; }
        }

        private string interviewId;
        private Identity groupIdentity;
        private NavigationState navigationState;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public GroupViewModel NavigateToGroupViewModel { get; private set; }

        public GroupNavigationViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            GroupViewModel navigateToGroupViewModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.NavigateToGroupViewModel = navigateToGroupViewModel;
        }

        public void Init(string interviewId, Identity groupIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.groupIdentity = groupIdentity;
            this.navigationState = navigationState;

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            this.IsInSection = !questionnaire.GroupsParentIdMap[groupIdentity.Id].HasValue;
            this.NavigateToIdentity = this.GetNavigateToIdentity(this.IsInSection, questionnaire);

            if (this.NavigateToIdentity != null)
            {
                this.NavigateToGroupViewModel.Init(this.interviewId, this.NavigateToIdentity, this.navigationState);
            }

            this.Title = this.IsInSection ? UIResources.Interview_NextSection_ButtonText : UIResources.Interview_PreviousGroupNavigation_ButtonText;
        }

        private Identity NavigateToIdentity { get; set; }

        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        private bool isInSection;
        public bool IsInSection
        {
            get { return this.isInSection; }
            set { this.isInSection = value; RaisePropertyChanged(); }
        }

        private string title;

        public IMvxCommand NavigateCommand
        {
            get { return new MvxCommand(async () => await this.Navigate(), () => NavigateToIdentity != null); }
        }

        private async Task Navigate()
        {
            if (IsInSection)
            {
                await navigationState.NavigateTo(NavigateToIdentity);
            }
            else
            {
                await navigationState.NavigateTo(NavigateToIdentity, groupIdentity);
            }
        }

        private Identity GetNavigateToIdentity(bool inSection, QuestionnaireModel questionnaire)
        {
            if (inSection)
            {
                int currentSectionIndex = questionnaire.GroupsHierarchy.FindIndex(x => x.Id == this.groupIdentity.Id);

                if (currentSectionIndex >= questionnaire.GroupsHierarchy.Count - 1)
                {
                    return null;
                }
                else
                {
                    return new Identity(questionnaire.GroupsHierarchy[currentSectionIndex + 1].Id, new decimal[0]);
                }
            }
            else
            {
                var parentId = questionnaire.GroupsParentIdMap[this.groupIdentity.Id].Value;
                int rosterLevelOfParent = questionnaire.GroupsRosterLevelDepth[this.groupIdentity.Id];
                decimal[] parentRosterVector = this.groupIdentity.RosterVector.Take(rosterLevelOfParent).ToArray();
                return new Identity(parentId, parentRosterVector);
            }
        }
    }
}
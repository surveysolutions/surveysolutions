using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels
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
            this.NavigateToIdentity = this.GetNavigateToIdentity(questionnaire);

            if (this.NavigateToIdentity != null)
            {
                this.NavigateToGroupViewModel.Init(this.interviewId, this.NavigateToIdentity, groupIdentity, this.navigationState);
            }
        }

        private Identity NavigateToIdentity { get; set; }

        private bool isInSection;
        public bool IsInSection
        {
            get { return this.isInSection; }
            set { this.isInSection = value; RaisePropertyChanged(); }
        }

        public IMvxCommand NavigateCommand
        {
            get { return new MvxCommand(async () => await this.NavigateAsync(), () => NavigateToIdentity != null); }
        }

        private async Task NavigateAsync()
        {
            if (IsInSection)
            {
                await navigationState.NavigateToAsync(NavigateToIdentity);
            }
            else
            {
                await navigationState.NavigateToAsync(NavigateToIdentity, groupIdentity);
            }
        }

        private Identity GetNavigateToIdentity(QuestionnaireModel questionnaire)
        {
            if (this.IsInSection)
            {
                return this.GetNextSectionIdentity(questionnaire);
            }
            else
            {
                return this.GetParentIdentity(questionnaire);
            }
        }

        private Identity GetParentIdentity(QuestionnaireModel questionnaire)
        {
            var parentId = questionnaire.GroupsParentIdMap[this.groupIdentity.Id].Value;
            int rosterLevelOfParent = questionnaire.GroupsRosterLevelDepth[this.groupIdentity.Id];
            decimal[] parentRosterVector = this.groupIdentity.RosterVector.Take(rosterLevelOfParent).ToArray();
            return new Identity(parentId, parentRosterVector);
        }

        private Identity GetNextSectionIdentity(QuestionnaireModel questionnaire)
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
    }
}
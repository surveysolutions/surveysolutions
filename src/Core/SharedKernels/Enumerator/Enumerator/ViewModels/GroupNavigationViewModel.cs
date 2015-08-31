using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class GroupNavigationViewModel : MvxNotifyPropertyChanged
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

        protected GroupNavigationViewModel(){}

        public GroupNavigationViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            GroupViewModel navigateToGroupViewModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.NavigateToGroupViewModel = navigateToGroupViewModel;
        }

        public virtual void Init(string interviewId, Identity groupIdentity, NavigationState navigationState)
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
            set { this.isInSection = value; this.RaisePropertyChanged(); }
        }

        public IMvxCommand NavigateCommand
        {
            get { return new MvxCommand(async () => await this.NavigateAsync(), () => this.NavigateToIdentity != null); }
        }

        private async Task NavigateAsync()
        {
            if (this.IsInSection)
            {
                await this.navigationState.NavigateToAsync(new NavigationIdentity(this.NavigateToIdentity));
            }
            else
            {
                await this.navigationState.NavigateToAsync(new NavigationIdentity(this.NavigateToIdentity, anchoredElementIdentity: this.groupIdentity));
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
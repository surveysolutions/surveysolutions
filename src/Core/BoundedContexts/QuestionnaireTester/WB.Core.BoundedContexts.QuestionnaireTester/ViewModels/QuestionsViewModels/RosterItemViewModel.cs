using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RosterItemViewModel : MvxNotifyPropertyChanged
    {
        private NavigationState navigationState;
        private Identity rosterIdentity;

        public string QuestionnaireRosterTitle { get; set; }
        public string InterviewRosterTitle { get; set; }
        public int AnsweredQuestionsCount { get; set; }
        public int SubgroupsCount { get; set; }
        public int QuestionsCount { get; set; }

        private IMvxCommand navigateToRosterCommand;
        public IMvxCommand NavigateToRosterCommand
        {
            get { return navigateToRosterCommand ?? (navigateToRosterCommand = new MvxCommand(NavigateToRoster)); }
        }

        public void Init(Identity rosterIdentity, NavigationState navigationState)
        {
            this.navigationState = navigationState;
            this.rosterIdentity = rosterIdentity;
        }

        private void NavigateToRoster()
        {
            this.navigationState.NavigateTo(this.rosterIdentity);
        }
    }
}
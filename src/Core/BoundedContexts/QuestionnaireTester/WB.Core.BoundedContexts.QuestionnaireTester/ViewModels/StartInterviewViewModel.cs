using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class StartInterviewViewModel : MvxViewModel,
        IInterviewEntityViewModel
    {
        private string interviewId;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
        }

        private IMvxCommand startInterviewCommand;
        public IMvxCommand StartInterviewCommand
        {
            get
            {
                return startInterviewCommand ?? (startInterviewCommand = new MvxCommand(this.StartInterview));
            }
        }

        private void StartInterview()
        {
            this.ShowViewModel<InterviewViewModel>(new { interviewId = this.interviewId });
        }
    }
}
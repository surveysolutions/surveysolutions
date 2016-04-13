using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class StaticTextStateViewModel : MvxNotifyPropertyChanged
    {
        public StaticTextStateViewModel(EnablementViewModel enablement)
        {
            this.Enablement = enablement;
        }

        public EnablementViewModel Enablement { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Enablement.Init(interviewId, entityIdentity, navigationState);
        }
    }
}
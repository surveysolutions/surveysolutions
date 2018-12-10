using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class PlainRosterTitleViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel
    {
        public DynamicTextViewModel Title { get; }

        public PlainRosterTitleViewModel(DynamicTextViewModel title)
        {
            this.Title = title;
        }
        public Identity Identity { get; private set; }
        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Identity = entityIdentity;
            Title.Init(interviewId, entityIdentity);
        }
    }
}

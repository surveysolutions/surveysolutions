using System.Diagnostics.CodeAnalysis;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    [ExcludeFromCodeCoverage] // no reason to cover it yet. Doesn't have any logic 
    public class PlainRosterTitleViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel
    {
        public DynamicTextViewModel Title { get; }
        public EnablementViewModel Enablement { get; }

        public PlainRosterTitleViewModel(DynamicTextViewModel title, EnablementViewModel enablement)
        {
            this.Title = title;
            Enablement = enablement;
        }
        public Identity Identity { get; private set; }
        public virtual void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Identity = entityIdentity;
            Title.Init(interviewId, entityIdentity);
            Enablement.Init(interviewId, entityIdentity);
        }
    }
}

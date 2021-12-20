using Android.Runtime;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    [MvxFragmentPresentation]
    [Register(nameof(CompleteInterviewFragment))]
    public class CompleteInterviewFragment : BaseFragment<CompleteInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_complete;
    }
}

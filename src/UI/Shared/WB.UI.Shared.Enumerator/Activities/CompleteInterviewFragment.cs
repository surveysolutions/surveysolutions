using Android.Runtime;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewFragment")]
    public class CompleteInterviewFragment : BaseFragment<CompleteInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_complete;
    }
}

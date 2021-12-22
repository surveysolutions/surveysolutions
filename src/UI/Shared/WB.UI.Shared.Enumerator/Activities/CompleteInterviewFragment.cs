using Android.Runtime;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewFragment")]
    public class CompleteInterviewFragment : BaseFragment<CompleteInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_complete;
    }
}

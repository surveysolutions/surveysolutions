using Android.Runtime;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Register("wb.ui.interviewer.activities.DashboardCompletednterviewsFragment")]
    public class DashboardCompletednterviewsFragment : BaseFragment<CompletedInterviewsViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.fragment_dashboard_tab;
    }
}
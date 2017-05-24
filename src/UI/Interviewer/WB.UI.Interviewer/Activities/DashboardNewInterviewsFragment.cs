using Android.Runtime;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;

namespace WB.UI.Interviewer.Activities
{
    [Register("wb.ui.interviewer.activities.DashboardNewInterviewsFragment")]
    public class DashboardNewInterviewsFragment : RecyclerViewFragment<NewInterviewsViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.fragment_dashboard_tab_new;
    }
}
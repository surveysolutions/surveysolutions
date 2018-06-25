using Android.Runtime;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    [Register("wb.ui.interviewer.activities.dashboard.QuestionnairesFragment")]
    public class QuestionnairesFragment : RecyclerViewFragment<CreateNewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.dashboard_assignments_tab;
    }
}

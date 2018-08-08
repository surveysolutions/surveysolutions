using Android.OS;
using MvvmCross.Droid.Support.V7.RecyclerView;
using WB.UI.Interviewer.CustomControls;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    public class InterviewerSearchActivity : SearchActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.dashboard_tab_recycler);
            recyclerView.ItemTemplateSelector = new InterviewerDashboardTemplateSelector();
        }
    }
}

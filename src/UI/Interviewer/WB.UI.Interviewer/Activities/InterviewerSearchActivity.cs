using Android.Views;
using MvvmCross.DroidX.RecyclerView;
using WB.UI.Interviewer.CustomControls;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "", 
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateVisible,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        NoHistory = true,
        Exported = false)]
    public class InterviewerSearchActivity : SearchActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.dashboard_tab_recycler);
            recyclerView.ItemTemplateSelector = new InterviewerDashboardTemplateSelector();
            recyclerView.SetItemAnimator(null);
        }
    }
}

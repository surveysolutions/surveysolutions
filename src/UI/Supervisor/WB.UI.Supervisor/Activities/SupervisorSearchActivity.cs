using Android.App;
using Android.OS;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Supervisor.CustomControls;

namespace WB.UI.Supervisor.Activities
{
    [Activity(Label = "", 
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateVisible,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class SupervisorSearchActivity : SearchActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.dashboard_tab_recycler);
            recyclerView.ItemTemplateSelector = new SupervisorDashboardTemplateSelector();
        }
    }
}

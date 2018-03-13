using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.UI.Interviewer.Services;
using WB.UI.Shared.Enumerator.Activities;
using MvxFragmentStatePagerAdapter = WB.UI.Interviewer.CustomControls.MvxFragmentStatePagerAdapter;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    [Activity(Label = "", 
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateVisible,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DashboardSearchActivity : BaseActivity<DashboardSearchViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.dashboard_search;

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnCreate(Bundle bundle)
        {
            Mvx.Trace("Dashboard Search activity started");
            base.OnCreate(bundle);
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
        }

        public override void OnBackPressed() {}

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return base.OnOptionsItemSelected(item);
        }
    }
}

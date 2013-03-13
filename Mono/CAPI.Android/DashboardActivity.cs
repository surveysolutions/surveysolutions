using Android.App;
using Android.OS;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Simple;

namespace CAPI.Android
{
    [Activity(Label = "CAPI", Icon = "@drawable/capi")]
    public class DashboardActivity : MvxSimpleBindingActivity<DashboardModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
          
            base.OnCreate(bundle);
            if (this.FinishIfNotLoggedIn())
                return;
            ViewModel =
                CapiApplication.LoadView<DashboardInput, DashboardModel>(
                    new DashboardInput(CapiApplication.Membership.CurrentUser.Id));
            SetContentView(Resource.Layout.Main);
        }

        public override bool OnCreateOptionsMenu(global::Android.Views.IMenu menu)
        {
            this.CreateActionBar();
            return base.OnCreateOptionsMenu(menu);
        }
    }
}


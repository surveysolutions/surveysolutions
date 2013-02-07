using Android.App;
using Android.OS;
using AndroidApp.Core.Model.ViewModel.Dashboard;
using AndroidApp.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Simple;
namespace AndroidApp
{
    [Activity(Label = "CAPI",  Icon = "@drawable/capi")]
    public class DashboardActivity : MvxSimpleBindingActivity<DashboardModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (CapiApplication.Membership.CurrentUser == null)
                StartActivity(typeof (LoginActivity));
            ViewModel =
                CapiApplication.LoadView<DashboardInput, DashboardModel>(
                    new DashboardInput(CapiApplication.Membership.CurrentUser.Id));
            SetContentView(Resource.Layout.Main);

        }
        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            this.CreateActionBar();
            return base.OnCreateOptionsMenu(menu);
        }
    }
}


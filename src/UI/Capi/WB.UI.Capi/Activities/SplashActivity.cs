using Android.App;
using Android.Content.PM;
using WB.UI.Capi.ViewModel;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Capi.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : BaseActivity<SplashViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.splash; }
        }
    }
}
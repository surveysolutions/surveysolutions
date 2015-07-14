using Android.App;
using Android.Content.PM;
using WB.Core.BoundedContexts.Tester.ViewModels;

namespace WB.UI.Tester.Activities
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
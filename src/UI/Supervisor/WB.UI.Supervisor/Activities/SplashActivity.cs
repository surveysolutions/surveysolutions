using Android.App;
using Android.Content.PM;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Supervisor.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    [MvxActivityPresentation]
    public class SplashActivity : EnumeratorSplashScreenAppCompatActivity<SupervisorSetup, SupervisorMvxApplication>
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }
    }
}

using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace WB.UI.Supervisor.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    [MvxActivityPresentation]
    public class SplashActivity : MvxSplashScreenAppCompatActivity<SupervisorSetup, SupervisorMvxApplication>
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }
    }
}

using Android.App;
using Android.Content.PM;

namespace WB.UI.Tester.Activities
{
    [Activity(NoHistory = true, 
        MainLauncher = true, 
        ScreenOrientation = ScreenOrientation.Portrait, 
        Theme = "@style/AppTheme")]
    public class SplashActivity : MvvmCross.Platforms.Android.Views.MvxSplashScreenActivity<Setup, TesterMvxApplication>
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }
    }
}

using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace WB.UI.Tester.Activities
{
    [Activity(NoHistory = true, 
        MainLauncher = true, 
        ScreenOrientation = ScreenOrientation.Portrait, 
        Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenAppCompatActivity<TesterSetup, TesterMvxApplication>
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }
    }
}

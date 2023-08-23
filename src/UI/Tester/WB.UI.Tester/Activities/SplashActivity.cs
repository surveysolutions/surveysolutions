using Android.App;
using Android.Content.PM;
using MvvmCross.Platforms.Android.Views;

namespace WB.UI.Tester.Activities
{
    [Activity(NoHistory = true, 
        MainLauncher = true, 
        ScreenOrientation = ScreenOrientation.Portrait, 
        Theme = "@style/AppTheme")]
    public class SplashActivity : MvxStartActivity
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }
    }
}

using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Views;

namespace WB.UI.Interviewer.Activities
{
    [Activity(NoHistory = true, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme")]
    public class SplashActivity : MvxSplashScreenAppCompatActivity<InterviewerSetup, InterviewerMvxApplication>
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }

        public override void InitializationComplete()
        {
            base.InitializationComplete();

        }
    }
}

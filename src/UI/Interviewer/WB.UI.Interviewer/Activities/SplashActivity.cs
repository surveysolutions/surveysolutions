using Android.App;
using Android.Content.PM;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(NoHistory = true, 
        MainLauncher = true, 
        ScreenOrientation = ScreenOrientation.Portrait, 
        Theme = "@style/AppTheme")]
    public class SplashActivity : EnumeratorSplashScreenAppCompatActivity<InterviewerSetup, InterviewerMvxApplication>
    {
        public SplashActivity() : base(Resource.Layout.splash)
        {
        }
    }
}

using Android.App;
using Android.Content.PM;
using Android.OS;
using MvvmCross.Platforms.Android.Views;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
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

        /// <summary>
        /// Finishes if not at root
        /// https://stackoverflow.com/questions/19545889/app-restarts-rather-than-resumes/23220151#23220151
        /// </summary>
        /// <param name="bundle">Bundle.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            if (!IsTaskRoot)
            {
                Finish();
            }
        }
    }
}

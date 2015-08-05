using Android.App;
using Android.Content;
using Android.Views;
using WB.UI.Tester.Activities;

namespace WB.UI.Capi.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : BaseInterviewActivity
    {
        protected override int MenuResourceId { get { return Resource.Menu.interview; } }

        protected override void OnMenuItemSelected(int resourceId)
        {
            switch (resourceId)
            {
                case Resource.Id.interview_dashboard:
                    this.StartActivity(new Intent(this, typeof(DashboardActivity)));
                    break;
                case Resource.Id.interview_settings:
                    this.StartActivity(new Intent(this, typeof(PrefsActivity)));
                    break;
                case Resource.Id.interview_signout:
                    this.StartActivity(new Intent(this, typeof(LoginActivity)));
                    break;

            }
        }
    }
}
using Android.App;
using Android.Views;
using WB.UI.Capi.ViewModel;
using WB.UI.Tester.Activities;

namespace WB.UI.Capi.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : EnumeratorInterviewActivity<InterviewerInterviewViewModel>
    {
        protected override int MenuResourceId { get { return Resource.Menu.interview; } }

        protected override void OnMenuItemSelected(int resourceId)
        {
            switch (resourceId)
            {
                case Resource.Id.interview_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
                case Resource.Id.interview_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
        }
    }
}
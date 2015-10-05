using Android.App;
using Android.Views;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : EnumeratorInterviewActivity<InterviewerInterviewViewModel>
    {
        protected override int MenuResourceId { get { return Resource.Menu.interview; } }

        public override async void OnBackPressed()
        {
            await this.ViewModel.NavigateToPreviousViewModelAsync(() =>
                {
                    Application.SynchronizationContext.Post(async _ => { await this.ViewModel.NavigateBack(); }, null);
                    this.Finish();
                });
        }

        protected override void OnMenuItemSelected(int resourceId)
        {
            switch (resourceId)
            {
                case Resource.Id.interview_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
                case Resource.Id.menu_troubleshooting:
                    this.ViewModel.NavigateToTroubleshootingPageCommand.Execute();
                    break;
                case Resource.Id.interview_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
            this.Finish();
        }
    }
}
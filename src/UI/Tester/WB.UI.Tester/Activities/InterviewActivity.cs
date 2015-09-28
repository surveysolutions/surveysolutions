using Android.App;
using Android.Content;
using Android.Views;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : EnumeratorInterviewActivity<InterviewViewModel>
    {
        protected override int MenuResourceId { get { return Resource.Menu.interview; } }

        public override async void OnBackPressed()
        {
            await this.ViewModel.NavigateToPreviousViewModel(() =>
            {
                Application.SynchronizationContext.Post(_ => { this.ViewModel.NavigateBack(); }, null);
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
                case Resource.Id.interview_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.interview_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
            this.Finish();
        }
    }
}
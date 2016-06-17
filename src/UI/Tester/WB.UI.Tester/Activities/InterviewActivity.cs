using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : EnumeratorInterviewActivity<InterviewViewModel>
    {
        protected override int MenuResourceId => Resource.Menu.interview;

        public override void OnBackPressed()
        {
            this.ViewModel.NavigateToPreviousViewModel(() =>
            {
                this.ViewModel.NavigateBack();
                this.Finish();
            });
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(this.MenuResourceId, menu);

            menu.LocalizeMenuItem(Resource.Id.interview_dashboard, TesterUIResources.MenuItem_Title_Dashboard);
            menu.LocalizeMenuItem(Resource.Id.interview_settings, TesterUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.interview_signout, TesterUIResources.MenuItem_Title_SignOut);

            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnMenuItemSelected(int resourceId)
        {
            switch (resourceId)
            {
                case Resource.Id.interview_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
                case Resource.Id.interview_settings:
                    this.ViewModel.NavigateToSettingsCommand.Execute();
                    break;
                case Resource.Id.interview_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
            }
        }
    }
}
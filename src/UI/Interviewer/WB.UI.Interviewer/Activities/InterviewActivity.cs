using Android.App;
using Android.Views;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : EnumeratorInterviewActivity<InterviewerInterviewViewModel>
    {
        private ICommandService CommandService
        {
            get { return ServiceLocator.Current.GetInstance<ICommandService>(); }   
        }

        protected override int MenuResourceId { get { return Resource.Menu.interview; } }

        public override async void OnBackPressed()
        {
            await this.ViewModel.NavigateToPreviousViewModelAsync(() =>
                {
                    Application.SynchronizationContext.Post(async _ => { await this.ViewModel.NavigateBack(); }, null);
                    this.Finish();
                });
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(this.MenuResourceId, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_dashboard, InterviewerUIResources.MenuItem_Title_Dashboard);
            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);

            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);
            return base.OnCreateOptionsMenu(menu);
        }

        protected override async void OnMenuItemSelected(int resourceId)
        {
            await this.CommandService.WaitPendingCommandsAsync().ConfigureAwait(false);

            switch (resourceId)
            {
                case Resource.Id.menu_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
            }

            this.Finish();
        }
    }
}
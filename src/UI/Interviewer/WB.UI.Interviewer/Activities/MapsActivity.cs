using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Interviewer.Services;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MapsActivity : BaseActivity<MapsViewModel>, IMapSyncBackgroundService
    {
        public MapDownloadBackgroundServiceBinder Binder { get; set; }

        protected override int ViewResourceId
        {
            get { return Resource.Layout.maps; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.ViewModel.Synchronization.MapSyncBackgroundService = this;
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.BindService(new Intent(this, typeof(MapDownloadBackgroundService)), new MapDownloadServiceConnection(this), Bind.AutoCreate);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.maps, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);
            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_dashboard, InterviewerUIResources.MenuItem_Title_Dashboard);

            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
                case Resource.Id.menu_map_synchronization:
                    this.ViewModel.MapSynchronizationCommand.Execute();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void SyncMaps() => this.Binder.GetMapDownloadBackgroundService().SyncMaps();

        public MapSyncProgressStatus CurrentProgress => this.Binder.GetMapDownloadBackgroundService().CurrentProgress;
    }
}

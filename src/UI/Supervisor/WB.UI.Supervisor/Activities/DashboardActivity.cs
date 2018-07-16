using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Supervisor.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [MvxActivityPresentation]
    public class DashboardActivity : BaseActivity<DashboardViewModel>, 
        ISyncBgService<SyncProgressDto>, ISyncServiceHost<SyncBgService>
    {
        private ActionBarDrawerToggle drawerToggle;

        public DrawerLayout DrawerLayout { get; private set; }
        protected override int ViewResourceId => Resource.Layout.dashboard;

        ServiceBinder<SyncBgService> ISyncServiceHost<SyncBgService>.Binder { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(toolbar);
            SupportActionBar.SetDefaultDisplayHomeAsUpEnabled(false);

            this.DrawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            this.drawerToggle = new ActionBarDrawerToggle(this, DrawerLayout, toolbar, 0, 0);
            DrawerLayout.AddDrawerListener(drawerToggle);

            if (bundle == null)
            {
                if (this.ViewModel.LastVisitedInterviewId.HasValue)
                {
                    var dashboardItemAccessor = ServiceLocator.Current.GetInstance<IDashboardItemsAccessor>();

                    if (dashboardItemAccessor.IsOutboxInterview(this.ViewModel.LastVisitedInterviewId.Value))
                        this.ViewModel.ShowOutboxCommand.Execute();
                    if(dashboardItemAccessor.IsWaitingForSupervisorActionInterview(this.ViewModel.LastVisitedInterviewId.Value))
                        this.ViewModel.ShowWaitingSupervisorActionCommand.Execute();
                }
                else
                    ViewModel.ShowDefaultListCommand.Execute();
                ViewModel.ShowMenuViewModelCommand.Execute();
            }
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            this.drawerToggle.OnConfigurationChanged(newConfig);
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.BindService(new Intent(this, typeof(SyncBgService)), new SyncServiceConnection<SyncBgService>(this), Bind.AutoCreate);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.ViewModel.Synchronization.SyncBgService = this;
        }

        protected override void OnStop()
        {
            base.OnStop();
            Mvx.Resolve<IInterviewerSelectorDialog>()?.CloseDialog();
        }

        public override void OnBackPressed() { }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_offline_synchronization, "Offline Sync");
            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);
            menu.LocalizeMenuItem(Resource.Id.menu_maps, InterviewerUIResources.MenuItem_Title_Maps);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_maps:
                    this.ViewModel.NavigateToMapsCommand.Execute();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
                case Resource.Id.menu_offline_synchronization:
                    this.ViewModel.NavigateToOfflineSyncCommand.Execute();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        void ISyncBgService<SyncProgressDto>.StartSync() => ((ISyncServiceHost<SyncBgService>)this).Binder.GetService().StartSync();

        SyncProgressDto ISyncBgService<SyncProgressDto>.CurrentProgress => ((ISyncServiceHost<SyncBgService>)this).Binder.GetService().CurrentProgress;
    }
}

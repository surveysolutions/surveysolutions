using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Callbacks;
using WB.UI.Shared.Enumerator.Activities.Dashboard;
using WB.UI.Shared.Enumerator.Services;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Supervisor.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
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
                    var dashboardItemAccessor = this.ViewModel.DashboardItemsAccessor;

                    var interviewId = this.ViewModel.LastVisitedInterviewId.Value;
                    if (dashboardItemAccessor.IsOutboxInterview(interviewId))
                        this.ViewModel.ShowOutboxCommand.Execute();
                    if(dashboardItemAccessor.IsWaitingForSupervisorActionInterview(interviewId))
                        this.ViewModel.ShowWaitingSupervisorActionCommand.Execute();
                    if (dashboardItemAccessor.IsSentToInterviewer(interviewId))
                        this.ViewModel.ShowSentCommand.Execute();
                }
                else
                    ViewModel.ShowDefaultListCommand.Execute();
                ViewModel.ShowMenuViewModelCommand.Execute();
            }
            
            this.ViewModel.WorkspaceListUpdated += this.WorkspaceListUpdated;
        }

        protected override bool BackButtonCustomAction => true;
        protected override void BackButtonPressed()
        {
        }

        protected override void OnDestroy()
        {
            this.ViewModel.WorkspaceListUpdated -= this.WorkspaceListUpdated;

            base.OnDestroy();
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


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);
            
            dashboardMenu = menu;

            UpdateWorkspacesMenu();

            SetMenuItemIcon(menu, Resource.Id.menu_search, Resource.Drawable.dashboard_search_icon);
            SetMenuItemIcon(menu, Resource.Id.menu_sync_with_hq, Resource.Drawable.synchronize_icon);
            SetMenuItemIcon(menu, Resource.Id.menu_sync_offline, Resource.Drawable.receive_interviews_icon);

            menu.LocalizeMenuItem(Resource.Id.menu_search, EnumeratorUIResources.MenuItem_Title_Search);
            menu.LocalizeMenuItem(Resource.Id.menu_sync_with_hq, SupervisorUIResources.Synchronization_Synchronize_HQ);
            menu.LocalizeMenuItem(Resource.Id.menu_sync_offline, SupervisorUIResources.Synchronization_Synchronize_Offline);
            menu.LocalizeMenuItem(Resource.Id.menu_signout, EnumeratorUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, EnumeratorUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, EnumeratorUIResources.MenuItem_Title_Diagnostics);
            menu.LocalizeMenuItem(Resource.Id.menu_maps, EnumeratorUIResources.MenuItem_Title_Maps);
            return true;
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
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
                case Resource.Id.menu_search:
                    this.ViewModel.ShowSearchCommand.Execute();
                    break;
                case Resource.Id.menu_sync_with_hq:
                    this.ViewModel.SynchronizationCommand.Execute();
                    break;
                case Resource.Id.menu_sync_offline:
                    this.ViewModel.NavigateToOfflineSyncCommand.Execute();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        void ISyncBgService<SyncProgressDto>.StartSync() => ((ISyncServiceHost<SyncBgService>)this).Binder.GetService().StartSync();

        SyncProgressDto ISyncBgService<SyncProgressDto>.CurrentProgress => ((ISyncServiceHost<SyncBgService>)this).Binder.GetService().CurrentProgress;
        
        private void WorkspaceListUpdated(object sender, EventArgs e)
        {
            UpdateWorkspacesMenu();
        }

        private IMenu dashboardMenu;
        
        private void UpdateWorkspacesMenu()
        {
            IMenu menu = dashboardMenu;
            var workspaces = this.ViewModel.GetWorkspaces();
            var workspacesMenuItem = menu.FindItem(Resource.Id.menu_workspaces);
            if (workspacesMenuItem != null)
            {
                menu.LocalizeMenuItem(Resource.Id.menu_workspaces, EnumeratorUIResources.MenuItem_Title_Workspaces);

                var sub = workspacesMenuItem.SubMenu;
                sub.Clear();
                
                foreach (var workspace in workspaces)
                {
                    var menuItem = sub!.Add(workspace.DisplayName);
                    menuItem.SetCheckable(true);
                    menuItem.SetChecked(workspace.Name == ViewModel.CurrentWorkspace);
                    var workspaceName = workspace.Name;
                    menuItem.SetOnMenuItemClickListener(new MenuItemOnMenuItemClickListener(() =>
                    {
                        ViewModel.ChangeWorkspace(workspaceName);
                        return true;
                    }));
                }

                sub.Add(EnumeratorUIResources.MenuItem_Title_RefreshWorkspaces)
                    .SetOnMenuItemClickListener(new MenuItemOnMenuItemClickListener(() =>
                    {
                        ViewModel.RefreshWorkspaces();
                        return true;
                    }));
            }

            workspacesMenuItem.SetVisible(true);
        }
    }
}

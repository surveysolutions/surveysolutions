﻿using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
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
    public class DashboardActivity : BaseActivity<DashboardViewModel>, ISyncBgService, ISyncServiceHost
    {
        private ActionBarDrawerToggle drawerToggle;
        public DrawerLayout DrawerLayout { get; private set; }
        protected override int ViewResourceId => Resource.Layout.dashboard;

        public ServiceBinder<SyncBgService> Binder { get; set; }

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
                ViewModel.ShowDeafultListCommand.Execute();
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
            this.BindService(new Intent(this, typeof(SyncBgService)), new SyncServiceConnection(this), Bind.AutoCreate);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.ViewModel.Synchronization.SyncBgService = this;
        }

        public override void OnBackPressed() { }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_synchronization:
                    this.ViewModel.SynchronizationCommand.Execute();
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

        public void StartSync() => this.Binder.GetService().StartSync();

        public SyncProgressDto CurrentProgress => this.Binder.GetService().CurrentProgress;
    }
}

﻿using System.ComponentModel;
using Android.Content;
using Android.Gms.Nearby;
using Android.Views;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Snackbar;
using Google.Android.Material.Tabs;
using MvvmCross;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.UI.Interviewer.CustomControls;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Dashboard;
using WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Notifications;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>,
        ISyncBgService<SyncProgressDto>,
        ISyncServiceHost<SyncBgService>
    {
        private static Random rnd = new Random();

        protected override int ViewResourceId => Resource.Layout.dashboard;

        public ServiceBinder<SyncBgService> Binder { get; set; }

        private MvxFragmentStateAdapter fragmentStateAdapter;
        private ViewPager2 viewPager;
        
        private OnPageChangeCallback onPageChangeCallback;

        protected override void OnPause()
        {
            base.OnPause();
            this.RemoveFragments();
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.CreateFragments();

            this.RestoreGoogleApiConnectionIfNeeded();
            var notificationsPublisher = Mvx.IoCProvider.Resolve<INotificationPublisher>();
            notificationsPublisher.CancelAllNotifications(this);
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            this.RestoreGoogleApiConnectionIfNeeded();

            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var enumeratorSettings = Mvx.IoCProvider.Resolve<IEnumeratorSettings>();
            if (!enumeratorSettings.NotificationsEnabled) return;

            var notificationsCollector = Mvx.IoCProvider.Resolve<IInAppNotificationsCollector>();
            List<SimpleNotification> notifications = notificationsCollector.CollectInAppNotifications();

            if (notifications.Count <= 0) return;

            Snackbar snack = Snackbar.Make(toolbar, notifications[rnd.Next(notifications.Count)].ContentText,
                5000);
            snack.Show();
        }
        
        protected override bool BackButtonCustomAction => true;
        protected override void BackButtonPressed()
        {
        }

        private void RemoveFragments()
        {
            this.fragmentStateAdapter.RemoveAllFragments();
            this.fragmentStateAdapter = null;
            this.viewPager.Adapter = null;

            this.viewPager.UnregisterOnPageChangeCallback(onPageChangeCallback);
            
            this.ViewModel.StartedInterviews.PropertyChanged -= this.StartedInterviewsOnPropertyChanged;
            this.ViewModel.RejectedInterviews.PropertyChanged -= this.RejectedInterviewsOnPropertyChanged;
            this.ViewModel.CompletedInterviews.PropertyChanged -= this.CompletedInterviewsOnPropertyChanged;
            this.ViewModel.WorkspaceListUpdated -= this.WorkspaceListUpdated;
            this.ViewModel.WebInterviews.PropertyChanged -= this.WebInterviewInterviewsOnPropertyChanged;
        }

        private void CreateFragments()
        {
            this.viewPager = this.FindViewById<ViewPager2>(Resource.Id.pager);

            this.fragmentStateAdapter = new MvxFragmentStateAdapter(this, this.SupportFragmentManager, this.Lifecycle);
            this.viewPager.Adapter = this.fragmentStateAdapter;

            onPageChangeCallback = new OnPageChangeCallback(index =>
            {
                UpdateTypeOfInterviewsViewModelProperty(index);
            });

            this.viewPager.RegisterOnPageChangeCallback(onPageChangeCallback);

            this.ViewModel.StartedInterviews.PropertyChanged += this.StartedInterviewsOnPropertyChanged;
            this.ViewModel.RejectedInterviews.PropertyChanged += this.RejectedInterviewsOnPropertyChanged;
            this.ViewModel.CompletedInterviews.PropertyChanged += this.CompletedInterviewsOnPropertyChanged;
            this.ViewModel.WorkspaceListUpdated += this.WorkspaceListUpdated;
            this.ViewModel.WebInterviews.PropertyChanged += this.WebInterviewInterviewsOnPropertyChanged;

            this.fragmentStateAdapter.InsertFragment(typeof(QuestionnairesFragment), this.ViewModel.CreateNew,
                nameof(InterviewTabPanel.Title));

            var itemsCountPropertyCountName = nameof(ListViewModel.ItemsCount);

            this.StartedInterviewsOnPropertyChanged(this.ViewModel.StartedInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));
            this.RejectedInterviewsOnPropertyChanged(this.ViewModel.RejectedInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));
            this.CompletedInterviewsOnPropertyChanged(this.ViewModel.CompletedInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));
            this.WebInterviewInterviewsOnPropertyChanged(this.ViewModel.WebInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));

            var tabLayout = this.FindViewById<TabLayout>(Resource.Id.tabs);

            tabConfigurationStrategy = new TabConfigurationStrategy(fragmentStateAdapter);
            tabLayoutMediator = new TabLayoutMediator(tabLayout, this.viewPager, tabConfigurationStrategy);
            tabLayoutMediator.Attach();

            OpenRequestedTab();
        }

        public class TabConfigurationStrategy : Java.Lang.Object, TabLayoutMediator.ITabConfigurationStrategy
        {
            private readonly MvxFragmentStateAdapter fragmentStateAdapter;

            public TabConfigurationStrategy(MvxFragmentStateAdapter fragmentStateAdapter)
            {
                this.fragmentStateAdapter = fragmentStateAdapter;
            }

            public void OnConfigureTab(TabLayout.Tab tab, int position)
            {
                var fragment = (MvxFragment)fragmentStateAdapter.CreateFragment(position);
                    InterviewTabPanel viewModel = (InterviewTabPanel)fragment.ViewModel;
                    tab.SetText(viewModel.Title);
            }
        }
        
        private class OnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private Action<int> action;

            public OnPageChangeCallback(Action<int> action)
            {
                this.action = action;
            }

            public override void OnPageSelected(int position)
            {
                action?.Invoke(position);
                base.OnPageSelected(position);
            }

            protected override void Dispose(bool disposing)
            {
                action = null;
                base.Dispose(disposing);
            }
        }

        private void OpenRequestedTab()
        {
            for (int i = 0; i < this.fragmentStateAdapter.ItemCount; i++)
            {
                var fragment = (MvxFragment)fragmentStateAdapter.CreateFragment(i);
                InterviewTabPanel viewModel = (InterviewTabPanel)fragment.ViewModel;
                if (viewModel.DashboardType == this.ViewModel.TypeOfInterviews)
                {
                    this.viewPager.SetCurrentItem(i, false);
                    break;
                }
            }
        }

        private void WebInterviewInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<WebInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 4);

        private void CompletedInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<CompletedInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 3);

        private void RejectedInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<RejectedInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 2);

        private void StartedInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<StartedInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 1);

        private void UpdateFragmentByViewModelPropertyChange<TFragmentType>(ListViewModel listViewModel, string propertyName, int position)
        {
            if (propertyName != nameof(ListViewModel.ItemsCount)) return;

            if (!this.fragmentStateAdapter.HasFragmentForViewModel(listViewModel) && listViewModel.ItemsCount > 0)
            {
                this.fragmentStateAdapter.InsertFragment(typeof(TFragmentType), listViewModel,
                    nameof(InterviewTabPanel.Title), position);
            }

            if (this.fragmentStateAdapter.HasFragmentForViewModel(listViewModel) && listViewModel.ItemsCount == 0)
            {
                this.fragmentStateAdapter.RemoveFragmentByViewModel(listViewModel);
            }

            var viewPagerCurrentItem = viewPager.CurrentItem;
            if (viewPagerCurrentItem > 0)
            {
                UpdateTypeOfInterviewsViewModelProperty(viewPagerCurrentItem);
            }
        }

        private void UpdateTypeOfInterviewsViewModelProperty(int tabPosition)
        {
            var fragment = (MvvmCross.Platforms.Android.Views.Fragments.MvxFragment)this.fragmentStateAdapter.CreateFragment(tabPosition);
            var viewModel = (ListViewModel)fragment.ViewModel;
            this.ViewModel.TypeOfInterviews = viewModel.DashboardType;
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.BindService(new Intent(this, typeof(SyncBgService)),
                new SyncServiceConnection<SyncBgService>(this), Bind.AutoCreate);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.ViewModel.Synchronization.SyncBgService = this;
            this.ViewModel.OnOfflineSynchronizationStarted = OnOfflineSynchronizationStarted;
            this.ViewModel.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SynchronizationWithHqEnabled))
            {
                this.InvalidateOptionsMenu();
            }
        }

        private void WorkspaceListUpdated(object sender, EventArgs e)
        {
            UpdateWorkspacesDependentMenu();
        }

        private IMenu dashboardMenu;
        private TabConfigurationStrategy tabConfigurationStrategy;
        private TabLayoutMediator tabLayoutMediator;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);

            dashboardMenu = menu;

            SetMenuItemIcon(menu, Resource.Id.menu_search, Resource.Drawable.dashboard_search_icon);
            SetMenuItemIcon(menu, Resource.Id.menu_synchronization, Resource.Drawable.synchronize_icon);
            SetMenuItemIcon(menu, Resource.Id.menu_offline_synchronization, Resource.Drawable.synchronize_offline_icon);
            SetMenuItemIcon(menu, Resource.Id.menu_map_dashboard, Resource.Drawable.map_icon);

            if (ViewModel.SynchronizationWithHqEnabled)
            {
                menu.FindItem(Resource.Id.menu_offline_synchronization).SetVisible(false);
            }
            else
            {
                menu.FindItem(Resource.Id.menu_synchronization).SetVisible(false);
            }

            if (!ViewModel.DoesSupportMaps)
            {
                var mapDashboardMenu = menu.FindItem(Resource.Id.menu_map_dashboard);
                mapDashboardMenu.SetVisible(false);
            }
            
            UpdateWorkspacesDependentMenu();

            menu.LocalizeMenuItem(Resource.Id.menu_search, EnumeratorUIResources.MenuItem_Title_Search);
            menu.LocalizeMenuItem(Resource.Id.menu_signout, EnumeratorUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, EnumeratorUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, EnumeratorUIResources.MenuItem_Title_Diagnostics);
            menu.LocalizeMenuItem(Resource.Id.menu_maps, EnumeratorUIResources.MenuItem_Title_Maps);
            menu.LocalizeMenuItem(Resource.Id.menu_map_dashboard, EnumeratorUIResources.MenuItem_Title_Map_Dashboard);
            
            return base.OnCreateOptionsMenu(menu);
        }

        private void UpdateWorkspacesDependentMenu()
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
            
            
            var mapDashboardMenu = menu.FindItem(Resource.Id.menu_map_dashboard);
            var hasWorkspace = !string.IsNullOrEmpty(ViewModel.CurrentWorkspace);
            mapDashboardMenu.SetEnabled(hasWorkspace);
            mapDashboardMenu.Icon.Mutate().Alpha = hasWorkspace ? 255 : 120;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
                case Resource.Id.menu_map_dashboard:
                    this.ViewModel.NavigateToMapDashboardCommand.Execute();
                    break;
                case Resource.Id.menu_maps:
                    this.ViewModel.NavigateToMapsCommand.Execute();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
                case Resource.Id.menu_search:
                    this.ViewModel.ShowSearchCommand.Execute();
                    break;
                case Resource.Id.menu_synchronization:
                    this.ViewModel.SynchronizationCommand.Execute();
                    break;
                case Resource.Id.menu_offline_synchronization:
                    this.ViewModel.StartOfflineSyncCommand.Execute();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void StartSync() => this.Binder.GetService().StartSync();

        public SyncProgressDto CurrentProgress => this.Binder.GetService().CurrentProgress;

        #region Offline synhronization

        const int RequestCodeRecoverPlayServices = 1001;
            
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.viewPager?.UnregisterOnPageChangeCallback(onPageChangeCallback);
                this.ViewModel.OnOfflineSynchronizationStarted = null;
                
                this.tabLayoutMediator?.Dispose();
                this.tabLayoutMediator = null;
                
                tabConfigurationStrategy?.Dispose();
                tabConfigurationStrategy = null;
                
                onPageChangeCallback?.Dispose();
                onPageChangeCallback = null;
            }

            base.Dispose(disposing);
        }

        private void OnOfflineSynchronizationStarted()
        {
            if (!this.CheckPlayServices()) return;
            
            this.RestoreGoogleApiConnectionIfNeeded();
            this.ViewModel.StartDiscoveryAsyncCommand.Execute();
        }

        /// <summary>
        /// Check the device to make sure it has the Google Play Services APK.
        /// If it doesn't, display a dialog that allows users to download the APK from the Google Play Store 
        /// or enable it in the device's system settings.
        /// </summary>
        /// <returns></returns>
        private bool CheckPlayServices()
        {
            var resultCode = ViewModel.GoogleApiService.GetPlayServicesConnectionStatus();
            if (resultCode == GoogleApiConnectionStatus.Success) return true;

            if (ViewModel.GoogleApiService.CanResolvePlayServicesErrorByUser(resultCode))
            {
                this.ViewModel.ShowSynchronizationError(UIResources.OfflineSync_InstallPlayServices);
                this.ViewModel.GoogleApiService.ShowGoogleApiErrorDialog(resultCode,
                    RequestCodeRecoverPlayServices);
            }
            else
                this.ViewModel.ShowSynchronizationError(UIResources.OfflineSync_DeviceNotSupported);

            return false;
        }

        private void RestoreGoogleApiConnectionIfNeeded()
        {
            var apiClientFactory = Mvx.IoCProvider.GetSingleton<IGoogleApiClientFactory>();
            if(apiClientFactory != null)
                apiClientFactory.ConnectionsClient = NearbyClass.GetConnectionsClient(this);
            // System.Diagnostics.Trace.Write("StartDiscoveryAsyncCommand call from  RestoreGoogleApiConnectionIfNeeded");
            
            // this.ViewModel.StartDiscoveryAsyncCommand.Execute();
        }
        #endregion
    }
}

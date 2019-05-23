﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using MvvmCross;
using MvvmCross.Droid.Support.V4;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.OfflineSync.Activities;
using WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Notifications;
using MvxFragmentStatePagerAdapter = WB.UI.Interviewer.CustomControls.MvxFragmentStatePagerAdapter;
using Toolbar = Android.Support.V7.Widget.Toolbar;

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
        ISyncServiceHost<SyncBgService>, 
        GoogleApiClient.IConnectionCallbacks, 
        GoogleApiClient.IOnConnectionFailedListener
    {
        private static Random rnd = new Random();

        protected override int ViewResourceId => Resource.Layout.dashboard;

        public ServiceBinder<SyncBgService> Binder { get; set; }

        private MvxFragmentStatePagerAdapter fragmentStatePagerAdapter;
        private ViewPager viewPager;

        protected override void OnPause()
        {
            base.OnPause();
            this.RemoveFragments();
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.CreateFragments();

            var notificationsPublisher = Mvx.IoCProvider.Resolve<INotificationPublisher>();
            notificationsPublisher.CancelAllNotifications(this);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var enumeratorSettings = Mvx.IoCProvider.Resolve<IEnumeratorSettings>();
            if (!enumeratorSettings.NotificationsEnabled) return;

            var notificationsCollector = Mvx.IoCProvider.Resolve<INotificationsCollector>();
            List<SimpleNotification> notifications = notificationsCollector.CollectInAppNotifications();

            if (notifications.Count <= 0) return;

            Snackbar snack = Snackbar.Make(toolbar, notifications[rnd.Next(notifications.Count)].ContentText,
                5000);
            snack.Show();
        }

        private void RemoveFragments()
        {
            this.fragmentStatePagerAdapter.RemoveAllFragments();
            this.fragmentStatePagerAdapter = null;
            this.viewPager.Adapter = null;

            this.viewPager.PageSelected -= this.ViewPager_PageSelected;

            this.ViewModel.StartedInterviews.PropertyChanged -= this.StartedInterviewsOnPropertyChanged;
            this.ViewModel.RejectedInterviews.PropertyChanged -= this.RejectedInterviewsOnPropertyChanged;
            this.ViewModel.CompletedInterviews.PropertyChanged -= this.CompletedInterviewsOnPropertyChanged;
        }

        private void CreateFragments()
        {
            this.viewPager = this.FindViewById<ViewPager>(Resource.Id.pager);

            this.fragmentStatePagerAdapter = new MvxFragmentStatePagerAdapter(this, this.SupportFragmentManager);
            this.viewPager.Adapter = this.fragmentStatePagerAdapter;
            this.viewPager.PageSelected += this.ViewPager_PageSelected;

            this.ViewModel.StartedInterviews.PropertyChanged += this.StartedInterviewsOnPropertyChanged;
            this.ViewModel.RejectedInterviews.PropertyChanged += this.RejectedInterviewsOnPropertyChanged;
            this.ViewModel.CompletedInterviews.PropertyChanged += this.CompletedInterviewsOnPropertyChanged;

            this.fragmentStatePagerAdapter.InsertFragment(typeof(QuestionnairesFragment), this.ViewModel.CreateNew,
                nameof(InterviewTabPanel.Title));

            var itemsCountPropertyCountName = nameof(ListViewModel.ItemsCount);

            this.StartedInterviewsOnPropertyChanged(this.ViewModel.StartedInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));
            this.RejectedInterviewsOnPropertyChanged(this.ViewModel.RejectedInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));
            this.CompletedInterviewsOnPropertyChanged(this.ViewModel.CompletedInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));

            var tabLayout = this.FindViewById<TabLayout>(Resource.Id.tabs);
            tabLayout.SetupWithViewPager(this.viewPager);

            OpenRequestedTab();
        }

        private void OpenRequestedTab()
        {
            for (int i = 0; i < this.fragmentStatePagerAdapter.Count; i++)
            {
                var fragment = (MvxFragment) fragmentStatePagerAdapter.GetItem(i);
                InterviewTabPanel viewModel = (InterviewTabPanel) fragment.ViewModel;
                if (viewModel.InterviewStatus == this.ViewModel.TypeOfInterviews)
                {
                    this.viewPager.SetCurrentItem(i, false);
                    break;
                }
            }
        }

        private void CompletedInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<CompletedInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 3);

        private void RejectedInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<RejectedInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 2);

        private void StartedInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<StartedInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 1);
        
        private void UpdateFragmentByViewModelPropertyChange<TFragmentType>(ListViewModel listViewModel, string propertyName, int position)
        {
            if (propertyName != nameof(ListViewModel.ItemsCount)) return;

            if (!this.fragmentStatePagerAdapter.HasFragmentForViewModel(listViewModel) && listViewModel.ItemsCount > 0)
            {
                this.fragmentStatePagerAdapter.InsertFragment(typeof(TFragmentType), listViewModel,
                    nameof(InterviewTabPanel.Title), position);
            }

            if (this.fragmentStatePagerAdapter.HasFragmentForViewModel(listViewModel) && listViewModel.ItemsCount == 0)
            {
                this.fragmentStatePagerAdapter.RemoveFragmentByViewModel(listViewModel);
            }


            var viewPagerCurrentItem = viewPager.CurrentItem;
            if (viewPagerCurrentItem > 0)
            {
                UpdateTypeOfInterviewsViewModelProperty(viewPagerCurrentItem);
            }
        }

        private void UpdateTypeOfInterviewsViewModelProperty(int tabPosition)
        {
            var fragment = (MvxFragment)this.fragmentStatePagerAdapter.GetItem(tabPosition);
            var viewModel = (ListViewModel)fragment.ViewModel;
            this.ViewModel.TypeOfInterviews = viewModel.InterviewStatus;
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.BindService(new Intent(this, typeof(SyncBgService)), new SyncServiceConnection<SyncBgService>(this), Bind.AutoCreate);
        }

        private void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            UpdateTypeOfInterviewsViewModelProperty(e.Position);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.ViewModel.Synchronization.SyncBgService = this;
            this.ViewModel.OnOfflineSynchronizationStarted += OnOfflineSynchronizationStarted;
            this.ViewModel.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SynchronizationWithHqEnabled))
            {
                this.InvalidateOptionsMenu();
            }
        }

        public override void OnBackPressed() {}

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);

            SetMenuItemIcon(menu, Resource.Id.menu_search, Resource.Drawable.dashboard_search_icon);
            SetMenuItemIcon(menu, Resource.Id.menu_synchronization, Resource.Drawable.synchronize_icon);
            SetMenuItemIcon(menu, Resource.Id.menu_offline_synchronization, Resource.Drawable.synchronize_offline_icon);

            if (ViewModel.SynchronizationWithHqEnabled)
            {
                menu.FindItem(Resource.Id.menu_offline_synchronization).SetVisible(false);
            }
            else
            {
                menu.FindItem(Resource.Id.menu_synchronization).SetVisible(false);
            }

            menu.LocalizeMenuItem(Resource.Id.menu_search, InterviewerUIResources.MenuItem_Title_Search);
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
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
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

        protected GoogleApiClient GoogleApi;
        const int RequestCodeRecoverPlayServices = 1001;
        private INearbyConnection communicator;
        private BluetoothReceiver bluetoothReceiver;

        protected override void OnStop()
        {
            this.communicator?.StopAll();
            this.GoogleApi?.Disconnect();
            if (this.bluetoothReceiver != null)
            {
                UnregisterReceiver(this.bluetoothReceiver);
                bluetoothReceiver.BluetoothDisabled -= OnBluetoothDisabled;
                bluetoothReceiver = null;
            }

            base.OnStop();
        }

        private void OnBluetoothDisabled(object sender, EventArgs e)
        {
            this.UnregisterReceiver(this.bluetoothReceiver);
            this.bluetoothReceiver.BluetoothDisabled -= OnBluetoothDisabled;
            this.bluetoothReceiver = null;

            this.RestoreGoogleApiConnectionIfNeeded();
        }

        public void OnConnected(Bundle connectionHint)
        {
            this.ViewModel.StartDiscoveryAsyncCommand.Execute();
        }

        public void OnConnectionSuspended(int cause)
        {
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.GoogleApi?.Dispose();
                this.GoogleApi = null;
                this.ViewModel.OnOfflineSynchronizationStarted -= this.OnOfflineSynchronizationStarted;
            }

            base.Dispose(disposing);
        }

        private void OnOfflineSynchronizationStarted(object sender, EventArgs e)
        {
            if (!this.CheckPlayServices()) return;

            var mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (mBluetoothAdapter.IsEnabled)
            {
                bluetoothReceiver = new BluetoothReceiver();
                IntentFilter filter = new IntentFilter(BluetoothAdapter.ActionStateChanged);
                RegisterReceiver(bluetoothReceiver, filter);
                bluetoothReceiver.BluetoothDisabled += OnBluetoothDisabled;

                BluetoothAdapter.DefaultAdapter.Disable();
            }
            else
            {
                this.RestoreGoogleApiConnectionIfNeeded();
            }
        }

        /// <summary>
        /// Check the device to make sure it has the Google Play Services APK.
        /// If it doesn't, display a dialog that allows users to download the APK from the Google Play Store 
        /// or enable it in the device's system settings.
        /// </summary>
        /// <returns></returns>
        private bool CheckPlayServices()
        {
            GoogleApiAvailability apiAvailability = GoogleApiAvailability.Instance;
            int resultCode = apiAvailability.IsGooglePlayServicesAvailable(this);
            if (resultCode == ConnectionResult.Success) return true;

            if (apiAvailability.IsUserResolvableError(resultCode))
            {
                this.ViewModel.ShowSynchronizationError(UIResources.OfflineSync_InstallPlayServices);
                this.ViewModel.UserInteractionService.ShowGoogleApiErrorDialog(resultCode,
                    RequestCodeRecoverPlayServices);
            }
            else
                this.ViewModel.ShowSynchronizationError(UIResources.OfflineSync_DeviceNotSupported);

            return false;
        }

        private void RestoreGoogleApiConnectionIfNeeded()
        {
            if (this.GoogleApi == null)
            {
                this.GoogleApi = new GoogleApiClient.Builder(this)
                    .AddConnectionCallbacks(this)
                    .AddOnConnectionFailedListener(this)
                    .AddApi(NearbyClass.CONNECTIONS_API)
                    .Build();

                this.communicator = Mvx.IoCProvider.GetSingleton<INearbyConnection>();
                var apiClientFactory = Mvx.IoCProvider.GetSingleton<IGoogleApiClientFactory>();
                apiClientFactory.GoogleApiClient = this.GoogleApi;
            }

            if (this.GoogleApi.IsConnected)
            {
                System.Diagnostics.Trace.Write("StartDiscoveryAsyncCommand call from  RestoreGoogleApiConnectionIfNeeded");
                this.ViewModel.StartDiscoveryAsyncCommand.Execute();
                return;
            }

            this.GoogleApi.Connect();
        }
        #endregion
    }
}

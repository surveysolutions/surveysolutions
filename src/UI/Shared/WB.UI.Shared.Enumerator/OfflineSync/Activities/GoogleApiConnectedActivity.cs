﻿using System;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.OS;
using Android.Widget;
using MvvmCross;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation;

namespace WB.UI.Shared.Enumerator.OfflineSync.Activities
{
    public abstract class GoogleApiConnectedActivity<TViewModel>
        : BaseActivity<TViewModel>,
            GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
            where TViewModel : class, IMvxViewModel, IOfflineSyncViewModel
    {
        protected GoogleApiClient GoogleApi;
        const int RequestCodeRecoverPlayServices = 1001;
        private INearbyConnection communicator;
        private BluetoothReceiver bluetoothReceiver;

        protected override void OnResume()
        {
            base.OnResume();

            if (!this.CheckPlayServices()) return;

            var mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (mBluetoothAdapter?.IsEnabled == true)
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

        protected override void OnStop()
        {
            this.communicator?.StopAll();
            if (this.GoogleApi != null)
            {
                if (this.GoogleApi.IsConnected)
                {
                    this.GoogleApi.Disconnect();
                }
            }
            
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
                ViewModel.UserInteractionService.ShowGoogleApiErrorDialog(resultCode,
                    RequestCodeRecoverPlayServices,
                    () =>
                    {
                        this.Finish();
                        ViewModel.UserInteractionService.ShowToast(UIResources.OfflineSync_InstallPlayServices);
                    });
            }
            else
            {
                this.Finish();
                Toast.MakeText(this, UIResources.OfflineSync_DeviceNotSupported, ToastLength.Long).Show();
            }

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
                this.ViewModel.StartDiscoveryAsyncCommand.Execute();
                return;
            }

            if (!this.GoogleApi.IsConnected && !this.GoogleApi.IsConnecting)
                this.GoogleApi.Connect();
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
            }

            base.Dispose(disposing);
        }
    }
}

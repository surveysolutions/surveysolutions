using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.OS;
using Android.Widget;
using MvvmCross;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation;

namespace WB.UI.Shared.Enumerator.OfflineSync.Activities
{
    public abstract class GoogleApiConnectedActivity<TViewModel>
        : BaseActivity<TViewModel>
            where TViewModel : class, IMvxViewModel, IOfflineSyncViewModel
    {
        const int RequestCodeRecoverPlayServices = 1001;
        private INearbyConnection communicator;

        protected override void OnResume()
        {
            base.OnResume();

            if (!this.CheckPlayServices()) return;

            this.RestoreGoogleApiConnectionIfNeeded();
        }

        protected override void OnStop()
        {
            //this.communicator?.StopAll();
            //if (this.GoogleApi != null)
            //{
            //    if (this.GoogleApi.IsConnected)
            //    {
            //        this.GoogleApi.Disconnect();
            //    }
            //}

            base.OnStop();
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
                ViewModel.GoogleApiService.ShowGoogleApiErrorDialog(resultCode,
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
            this.communicator = Mvx.IoCProvider.GetSingleton<INearbyConnection>();
            var apiClientFactory = Mvx.IoCProvider.GetSingleton<IGoogleApiClientFactory>();
            apiClientFactory.ConnectionsClient = NearbyClass.GetConnectionsClient(this);

            this.ViewModel.StartDiscoveryAsyncCommand.Execute();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}

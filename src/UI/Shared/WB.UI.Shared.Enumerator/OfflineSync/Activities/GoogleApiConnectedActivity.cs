using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.OS;
using Android.Widget;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation;

namespace WB.UI.Shared.Enumerator.OfflineSync.Activities
{
    public class CancelDialogHandler : Java.Lang.Object, IDialogInterfaceOnCancelListener
    {
        private readonly Activity activity;

        public CancelDialogHandler(Activity activity)
        {
            this.activity = activity;
        }
        public void OnCancel(IDialogInterface dialog)
        {
            this.activity.Finish();
            Toast.MakeText(this.activity, UIResources.OfflineSync_InstallPlayServices, ToastLength.Short).Show();
        }
    }

    public abstract class GoogleApiConnectedActivity<TViewModel>
        : BaseActivity<TViewModel>,
            GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
            where TViewModel : class, IMvxViewModel, IOfflineSyncViewModel
    {
        protected GoogleApiClient GoogleApi;
        protected TaskCompletionSource<bool> ApiConnected;
        const int RequestCodeRecoverPlayServices = 1001;
        private INearbyConnection communicator;

        protected override void OnResume()
        {
            base.OnResume();

            if (!this.CheckPlayServices()) return;

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
                apiAvailability.GetErrorDialog(this, resultCode, RequestCodeRecoverPlayServices, new CancelDialogHandler(this)).Show();
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

                this.communicator = ServiceLocator.Current.GetInstance<INearbyConnection>();
                if (this.communicator is NearbyConnection gc)
                {
                    gc.SetGoogleApiClient(this.GoogleApi);
                }
            }

            if (this.GoogleApi.IsConnected) return;

            this.ApiConnected = new TaskCompletionSource<bool>();
            this.GoogleApi.Connect();

            this.ViewModel.SetGoogleAwaiter(this.ApiConnected.Task);
        }

        protected override void OnStop()
        {
            this.communicator?.StopAll();
            this.GoogleApi?.Disconnect();
            
            base.OnStop();
        }

        public void OnConnected(Bundle connectionHint)
        {
            this.ApiConnected?.TrySetResult(true);
        }

        public void OnConnectionSuspended(int cause)
        {
            
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            this.ApiConnected?.TrySetResult(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.GoogleApi?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

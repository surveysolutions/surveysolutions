using System.Threading.Tasks;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.OS;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
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
        protected TaskCompletionSource<bool> ApiConnected;
        private INearbyConnection communicator;

        protected override void OnCreate(Bundle bundle)
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

            var isGooglePlayServicesAvailable = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);

            if (isGooglePlayServicesAvailable != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(isGooglePlayServicesAvailable))
                {
                    GoogleApiAvailability.Instance.GetErrorDialog(this, isGooglePlayServicesAvailable, 9000).Show();
                }
            }
            
            RestoreGoogleApiConnectionIfNeeded();

            base.OnCreate(bundle);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.ViewModel.SetGoogleAwaiter(this.ApiConnected.Task);
        }

        private void RestoreGoogleApiConnectionIfNeeded()
        {
            if (GoogleApi.IsConnected) return;

            this.ApiConnected = new TaskCompletionSource<bool>();
            this.GoogleApi.Connect();
        }

        public void OnConnected(Bundle connectionHint)
        {
            ApiConnected?.TrySetResult(true);
        }

        protected override void OnStart()
        {
            if (!GoogleApi.IsConnected && !GoogleApi.IsConnecting)
            {
                RestoreGoogleApiConnectionIfNeeded();
            }

            base.OnStart();
        }

        protected override void OnStop()
        {
            this.communicator?.StopAllEndpoint();
            GoogleApi?.Disconnect();

            base.OnStop();
        }

        public void OnConnectionSuspended(int cause)
        {
            
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            ApiConnected?.TrySetResult(false);
        }
    }
}

using System.Threading.Tasks;
using Android.App;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.OS;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace WB.UI.Shared.Enumerator.OfflineSync.Activities
{
    public abstract class GoogleApiConnectedActivity<TViewModel> 
        : BaseActivity<TViewModel>
            where TViewModel : class, IMvxViewModel, IOfflineSyncViewModel
    {
        protected GoogleApiClient GoogleApi;
        protected TaskCompletionSource<bool> ApiConnected;
        
        protected override void OnCreate(Bundle bundle)
        {
            this.GoogleApi = new GoogleApiClient.Builder(this)
                .EnableAutoManage(this, result =>
                {
                    if (result.ErrorCode == ConnectionResult.ServiceVersionUpdateRequired)
                    {
                        new AlertDialog.Builder(this)
                            .SetTitle("Google API services require update")
                            .SetMessage("Offline synchronization require Google API Services . Please either update t")
                            .SetNegativeButton("Do nothing", (arg, sender) => { })
                            .SetPositiveButton("Open in Google Play", (arg, sender) =>
                            {
                                Dialog dialog = GoogleApiAvailability.Instance.GetErrorDialog(this,
                                    GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this), 1);
                                dialog.Show();
                            })
                            .SetNeutralButton("Search Google for Google Plays services APK", (arg, sender) => { })
                            .Create();
                    }

                    if (result.HasResolution)
                    {
                        result.StartResolutionForResult(this, result.ErrorCode);
                    }

                    this.ApiConnected.SetResult(false);
                })
                .AddConnectionCallbacks(() =>
                {
                    this.ApiConnected.SetResult(true);
                    this.ViewModel.OnGoogleApiReady();
                })
                .AddApi(NearbyClass.CONNECTIONS_API)
                .Build();
            
            var communicator = ServiceLocator.Current.GetInstance<INearbyConnection>();

            if (communicator is NearbyConnection gc)
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

            this.ApiConnected = new TaskCompletionSource<bool>();
            this.GoogleApi.Connect();

            base.OnCreate(bundle);
        }

        protected override void OnStart()
        {
            if (!GoogleApi.IsConnected && !GoogleApi.IsConnecting)
            {
                RestoreGoogleApiConnectionIfNeeded();
            }
            base.OnStart();
        }
        
        private void RestoreGoogleApiConnectionIfNeeded()
        {
            if (GoogleApi.IsConnected) return;

            this.ApiConnected = new TaskCompletionSource<bool>();
            this.GoogleApi.Connect();
        }
    }
}

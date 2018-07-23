using Android.Gms.Common.Apis;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    public class GoogleApiClientFactory : IGoogleApiClientFactory
    {
        public GoogleApiClient GoogleApiClient { get; set; }
    }
}
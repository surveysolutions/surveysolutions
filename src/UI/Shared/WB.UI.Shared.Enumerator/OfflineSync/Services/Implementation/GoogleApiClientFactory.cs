using Android.Gms.Common.Apis;
using Android.Gms.Nearby.Connection;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    public class GoogleApiClientFactory : IGoogleApiClientFactory
    {
        public IConnectionsClient ConnectionsClient { get; set; }
    }
}

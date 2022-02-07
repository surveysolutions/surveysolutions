using Android.Gms.Common.Apis;
using Android.Gms.Nearby.Connection;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    public interface IGoogleApiClientFactory
    {
        IConnectionsClient ConnectionsClient { get; set; }
    }
}

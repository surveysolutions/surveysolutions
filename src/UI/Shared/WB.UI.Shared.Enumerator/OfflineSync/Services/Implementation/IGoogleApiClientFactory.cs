using Android.Gms.Common.Apis;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    public interface IGoogleApiClientFactory
    {
        GoogleApiClient GoogleApiClient { get; set; }
    }
}
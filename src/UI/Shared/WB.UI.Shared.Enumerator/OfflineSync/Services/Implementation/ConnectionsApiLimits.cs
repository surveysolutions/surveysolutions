using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    public class ConnectionsApiLimits : IConnectionsApiLimits
    {
        public int MaxBytesLength => Android.Gms.Nearby.Connection.Connections.MaxBytesDataSize;
    }
}
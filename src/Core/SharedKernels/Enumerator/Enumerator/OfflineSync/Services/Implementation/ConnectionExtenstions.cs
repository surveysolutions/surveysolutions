using System.Linq;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public static class ConnectionExtenstions
    {
        public static string GetEndpointName(this INearbyConnection connection, string endpoint)
        {
            return connection.RemoteEndpoints?.SingleOrDefault(re => re.Enpoint == endpoint)?.Name ?? "";
        }
    }
}

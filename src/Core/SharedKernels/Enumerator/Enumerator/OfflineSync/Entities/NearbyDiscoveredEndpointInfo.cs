using System.Threading;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    public class NearbyDiscoveredEndpointInfo
    {
        public string EndpointName { get; set; }
        public string Endpoint { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

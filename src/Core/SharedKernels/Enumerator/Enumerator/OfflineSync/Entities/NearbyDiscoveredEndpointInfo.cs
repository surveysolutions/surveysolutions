using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    [ExcludeFromCodeCoverage] // because instantiated from UI projects
    public class NearbyDiscoveredEndpointInfo
    {
        public string EndpointName { get; set; }
        public string Endpoint { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

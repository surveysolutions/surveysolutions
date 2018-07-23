using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    [ExcludeFromCodeCoverage] // because instantiated from UI projects
    public class NearbyConnectionInfo
    {
        public string Endpoint { get; set; }
        public string EndpointName { get; set; }
        public bool IsIncomingConnection { get; set; }
        public string AuthenticationToken { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

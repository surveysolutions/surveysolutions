using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    [ExcludeFromCodeCoverage] // because instantiated from UI projects
    public class NearbyConnectionResolution
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Endpoint { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

using System.Threading;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    public class NearbyConnectionResolution
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Endpoint { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

using System.Threading;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    public class NearbyPayloadTransferUpdate
    {
        public long Id { get; set; }
        public long TotalBytes { get; set; }
        public long BytesTransferred { get; set; }
        public TransferStatus Status { get; set; }
        public string Endpoint { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}

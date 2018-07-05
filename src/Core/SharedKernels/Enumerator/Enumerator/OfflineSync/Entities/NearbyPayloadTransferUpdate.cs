namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    public class NearbyPayloadTransferUpdate
    {
        public long Id { get; set; }
        public long TotalBytes { get; set; }
        public long BytesTransferred { get; set; }
        public TransferStatus Status { get; set; }
    }
}

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public interface IChunkedByteArrayResponse : ICommunicationMessage
    {
        byte[] Content { get; set; }
        long Skipped { get; set; }
        int Length { get; set; }
        long Total { get; set; }
    }
}

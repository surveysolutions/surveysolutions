namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public interface IChunkedByteArrayResponse : ICommunicationMessage
    {
        byte[] Content { get; set; }
        long Skip { get; set; }
        int Length { get; set; }
        long Total { get; set; }
    }
}
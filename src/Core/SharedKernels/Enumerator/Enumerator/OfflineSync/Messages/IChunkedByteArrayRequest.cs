namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public interface IChunkedByteArrayRequest : ICommunicationMessage
    {
        // do not set manually
        long Maximum { get; set; }

        // do not set manually
        long Skip { get; set; }
    }
}
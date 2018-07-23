namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class SendBigAmountOfDataRequest : ICommunicationMessage
    {
        public byte[] Data { get; set; }
    }

    public class SendBigAmountOfDataResponse : ICommunicationMessage
    {
        public byte[] Data { get; set; }
    }
}

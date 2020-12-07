namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class FailedResponse : ICommunicationMessage
    {
        public string ErrorMessage { get; set; }
        public string Endpoint { get; set; }
        public string Error { get; set; }
        public string FailedPayload { get; set; }
    }
}
namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetLatestApplicationVersionRequest : ICommunicationMessage
    {

    }

    public class GetLatestApplicationVersionResponse : ICommunicationMessage
    {
        public int? InterviewerApplicationVersion { get; set; }
    }
}

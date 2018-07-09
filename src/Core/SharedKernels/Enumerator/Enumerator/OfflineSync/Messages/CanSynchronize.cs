namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class CanSynchronizeRequest : ICommunicationMessage
    {
        public CanSynchronizeRequest(int build)
        {
            this.InterviewerBuildNumber = build;
        }

        public int InterviewerBuildNumber { get; set; }
    }

    public class CanSynchronizeResponse : ICommunicationMessage
    {
        public bool CanSyncronize { get; set; }
    }
}

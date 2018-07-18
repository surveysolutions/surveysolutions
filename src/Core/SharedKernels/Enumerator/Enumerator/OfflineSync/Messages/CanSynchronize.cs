using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class CanSynchronizeRequest : ICommunicationMessage
    {
        public CanSynchronizeRequest(int build, Guid interviewerId, string token)
        {
            this.InterviewerBuildNumber = build;
            this.InterviewerId = interviewerId;
            this.Token = token;

        }

        public int InterviewerBuildNumber { get; set; }
        public Guid InterviewerId { set; get; }
        public string Token { get; set; }
    }

    public class CanSynchronizeResponse : ICommunicationMessage
    {
        public bool CanSyncronize { get; set; }

        public SyncDeclineReason Reason { get; set; }
    }
}

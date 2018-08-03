using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class CanSynchronizeRequest : ICommunicationMessage
    {
        public CanSynchronizeRequest(int build, Guid interviewerId, string secutrityStamp, long? lastHqSyncTimestamp)
        {
            this.InterviewerBuildNumber = build;
            this.InterviewerId = interviewerId;
            this.SecurityStamp = secutrityStamp;
            LastHqSyncTimestamp = lastHqSyncTimestamp;
        }

        public int InterviewerBuildNumber { get; set; }
        public Guid InterviewerId { get; set; }
        public string SecurityStamp { get; set; }
        public long? LastHqSyncTimestamp { get; set; }
    }

    public class CanSynchronizeResponse : ICommunicationMessage
    {
        public bool CanSyncronize { get; set; }

        public SyncDeclineReason Reason { get; set; }
    }
}

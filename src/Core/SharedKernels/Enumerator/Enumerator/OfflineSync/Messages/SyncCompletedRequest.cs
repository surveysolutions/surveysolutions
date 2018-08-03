using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class SyncCompletedRequest : ICommunicationMessage
    {
        public Guid InterviewerId { get; set; }
    }
}

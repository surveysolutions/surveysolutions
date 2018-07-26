using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class SendSyncProgressInfoRequest : ICommunicationMessage
    {
        public SyncProgressInfo Info { get; set; }
        public string InterviewerLogin { get; set; }
    }
}

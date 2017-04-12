using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog
{
    public class InterviewSyncLogSummary
    {
        public DateTime? FirstDownloadInterviewDate { get; set; }
        public DateTime? LastDownloadInterviewDate { get; set; }
        public DateTime? LastLinkDate { get; set; }
        public DateTime? LastUploadInterviewDate { get; set; }

        public bool WasDeviceLinkedAfterInterviewWasDownloaded 
            => LastLinkDate > LastDownloadInterviewDate;

        public bool InterviewWasNotDownloadedAfterItWasUploaded
            => LastDownloadInterviewDate < LastUploadInterviewDate;

        public bool IsInterviewOnDevice 
            => !LastUploadInterviewDate.HasValue || LastDownloadInterviewDate > LastUploadInterviewDate;

        public bool InterviewerChangedDeviceBetweenDownloads
            => FirstDownloadInterviewDate <= LastLinkDate && LastLinkDate <= LastDownloadInterviewDate;
    }
}

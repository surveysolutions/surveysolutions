using System;

namespace WB.Core.BoundedContexts.Headquarters.Troubleshooting.Views
{
    public class InterviewSyncLogSummary
    {
        public DateTime? FirstDownloadInterviewDate { get; set; }
        public DateTime? LastDownloadInterviewDate { get; set; }
        public DateTime? LastLinkDate { get; set; }
        public DateTime? LastUploadInterviewDate { get; set; }

        public bool WasDeviceLinkedAfterInterviewWasDownloaded 
            => this.LastLinkDate > this.LastDownloadInterviewDate;

        public bool InterviewWasNotDownloadedAfterItWasUploaded
            => this.LastDownloadInterviewDate < this.LastUploadInterviewDate;

        public bool IsInterviewOnDevice 
            => !this.LastUploadInterviewDate.HasValue || this.LastDownloadInterviewDate > this.LastUploadInterviewDate;

        public bool InterviewerChangedDeviceBetweenDownloads
            => this.FirstDownloadInterviewDate <= this.LastLinkDate && this.LastLinkDate <= this.LastDownloadInterviewDate;
    }
}

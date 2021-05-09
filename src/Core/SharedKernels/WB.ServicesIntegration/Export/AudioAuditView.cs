using System;

namespace WB.ServicesIntegration.Export
{
    public class AudioAuditView
    {
        public Guid InterviewId { get; set; }
        public AudioAuditFileView[] Files { get; set; } = new AudioAuditFileView[0];
    }

    public class AudioAuditFileView
    {
        public string FileName { get; set; } = String.Empty;
        public string ContentType { get; set; } = String.Empty;
    }
}

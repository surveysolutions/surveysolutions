using System;
using System.Collections.Generic;

namespace WB.Services.Export.Interview
{
    public class AudioAuditView
    {
        public Guid InterviewId { get; set; }
        public AudioAuditFileView[] Files { get; set; }
    }

    public class AudioAuditFileView
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}

using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewUploadState
    {
        public bool IsEventsUploaded { get; set; }
        public HashSet<string> ImagesFilesNames { get; set; } = new HashSet<string>();
        public HashSet<string> AudioFilesNames { get; set; } = new HashSet<string>();
        public HashSet<string> ImageQuestionsFilesMd5 { get; set; }
        public HashSet<string> AudioQuestionsFilesMd5 { get; set; }
        public HashSet<string> AudioAuditFilesMd5 { get; set; }
        public Guid? ResponsibleId { get; set; }
        public bool IsReceivedByInterviewer { get; set; }
        public InterviewMode Mode { get; set; }
    }
}

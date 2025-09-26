using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewUploadState
    {
        public bool IsEventsUploaded { get; set; }
        [Obsolete] public IReadOnlyCollection<string> ImagesFilesNames { get; } = new List<string>();
        [Obsolete] public IReadOnlyCollection<string> AudioFilesNames { get; } = new List<string>();
        [Obsolete] public IReadOnlyCollection<string> ImageQuestionsFilesMd5 { get; } = new List<string>();
        [Obsolete] public IReadOnlyCollection<string> AudioQuestionsFilesMd5 { get; } = new List<string>();
        [Obsolete] public IReadOnlyCollection<string> AudioAuditFilesMd5 { get; } = new List<string>();

        public List<FileInfo> ImagesFiles { get; set; } = new List<FileInfo>();
        public List<FileInfo> AudioFiles { get; set; } = new List<FileInfo>();
        public List<FileInfo> AudioAuditFiles { get; set; } = new List<FileInfo>();

        public Guid? ResponsibleId { get; set; }
        public bool IsReceivedByInterviewer { get; set; }
        public InterviewMode Mode { get; set; }
    }
    
    public record FileInfo(string FileName, string Md5);
}

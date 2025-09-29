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
        [Obsolete] public HashSet<string> ImageQuestionsFilesMd5 { get; set; } = new HashSet<string>();
        [Obsolete] public HashSet<string> AudioQuestionsFilesMd5 { get; set; } = new HashSet<string>();
        [Obsolete] public HashSet<string> AudioAuditFilesMd5 { get; set; } = new HashSet<string>();

        public List<FileInfoUploadState> ImagesFiles { get; set; } = new List<FileInfoUploadState>();
        public List<FileInfoUploadState> AudioFiles { get; set; } = new List<FileInfoUploadState>();
        public List<FileInfoUploadState> AudioAuditFiles { get; set; } = new List<FileInfoUploadState>();

        public Guid? ResponsibleId { get; set; }
        public bool IsReceivedByInterviewer { get; set; }
        public InterviewMode Mode { get; set; }
    }
    
    public record FileInfoUploadState(string FileName, string Md5);
}

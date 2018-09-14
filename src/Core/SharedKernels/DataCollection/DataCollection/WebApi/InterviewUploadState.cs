using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewUploadState
    {
        public bool IsEventsUploaded { get; set; }
        public HashSet<string> ImagesFilesNames { get; set; } = new HashSet<string>();
        public HashSet<string> AudioFilesNames { get; set; } = new HashSet<string>();
    }
}

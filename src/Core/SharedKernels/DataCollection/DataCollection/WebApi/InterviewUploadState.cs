using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewUploadState
    {
        public bool IsEventsUploaded { get; set; }
        public HashSet<string> BinaryFilesNames { get; set; } = new HashSet<string>();
    }
}
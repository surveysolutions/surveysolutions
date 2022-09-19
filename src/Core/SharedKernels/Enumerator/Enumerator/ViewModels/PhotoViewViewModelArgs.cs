using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class PhotoViewViewModelArgs
    {
        public Guid InterviewId { get; set; }
        public string FileName { get; set; }
        public Guid? AttachmentId { get; set; }
    }
}

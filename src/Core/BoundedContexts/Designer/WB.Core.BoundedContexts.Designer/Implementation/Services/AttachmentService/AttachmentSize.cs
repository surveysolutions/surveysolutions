using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentSize
    {
        public Guid AttachmentId { get; set; }
        public string ContentId { get; set; }
        public long Size { get; set; }
    }
}
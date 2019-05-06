using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentContent
    {
        public virtual string ContentId { get; set; }
        public virtual byte[] Content { get; set; }
        public virtual long Size { get; set; }
        public virtual AttachmentDetails Details { get; set; }
        public virtual string ContentType { get; set; }

        public virtual bool IsImage() => this.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase);
        public virtual bool IsAudio() => this.ContentType.StartsWith("audio", StringComparison.OrdinalIgnoreCase);
        public virtual bool IsVideo() => this.ContentType.StartsWith("video", StringComparison.OrdinalIgnoreCase);
        public virtual bool IsPdf() => this.ContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase);
    }
}

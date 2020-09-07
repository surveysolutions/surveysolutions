using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Questionnaire
{
    public class AttachmentContent
    {
        public virtual string ContentHash { get; set; }
        public virtual string ContentType { get; set; }
        public virtual byte[] Content { get; set; }
        public virtual string FileName { get; set; }

        public virtual bool IsImage()
        {
            return this.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase);
        }
        public virtual bool IsVideo()
        {
            return this.ContentType.StartsWith("video", StringComparison.OrdinalIgnoreCase);
        }
        public virtual bool IsAudio()
        {
            return this.ContentType.StartsWith("audio", StringComparison.OrdinalIgnoreCase);
        }
        public virtual bool IsPdf()
        {
            return this.ContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase);
        }
    }
}

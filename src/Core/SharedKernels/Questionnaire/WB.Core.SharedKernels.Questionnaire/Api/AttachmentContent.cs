using System;

namespace WB.Core.SharedKernels.Questionnaire.Api
{
    public class AttachmentContent 
    {
        public string Id { get; set; } = String.Empty;

        public string ContentType { get; set; } = String.Empty;

        public long Size { get; set; }

        public byte[]? Content { get; set; }

        public bool IsImage()
        {
            return this.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsVideo()
        {
            return this.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsPdf()
        {
            return this.ContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsAudio()
        {
            return this.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);
        }
    }
}

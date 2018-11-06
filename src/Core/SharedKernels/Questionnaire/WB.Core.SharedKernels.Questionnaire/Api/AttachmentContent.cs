using System;

namespace WB.Core.SharedKernels.Questionnaire.Api
{
    public class AttachmentContent 
    {
        public string Id { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }

        public byte[] Content { get; set; }

        public bool IsImage()
        {
            return this.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class QuestionnaireLiteInfo
    {
        public Guid Id { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }

}

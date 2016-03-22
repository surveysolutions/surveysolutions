using System;

namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class Attachment
    {
        public Guid AttachmentId { get; set; }
        public string Name { get; set; }
        public string ContentId { get; set; }

        public Attachment Clone()
        {
            return new Attachment
            {
                AttachmentId = this.AttachmentId,
                Name = this.Name,
                ContentId = this.ContentId
            };
        }
    }
}
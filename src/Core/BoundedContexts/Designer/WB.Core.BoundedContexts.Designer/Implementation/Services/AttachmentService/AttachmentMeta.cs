using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentMeta
    {
        public virtual Guid AttachmentId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual string ContentId { get; set; } = String.Empty;
        public virtual string FileName { get; set; } = String.Empty;
        public virtual DateTime LastUpdateDate { get; set; }
    }
}

using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentMeta
    {
        public virtual Guid AttachmentId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual string ContentId { get; set; }
        public virtual string FileName { get; set; }
        public virtual DateTime LastUpdateDate { get; set; }
    }
}

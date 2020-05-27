using System;

namespace WB.Core.BoundedContexts.Designer.Classifications
{
    public class ClassificationEntity : IClassificationEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string Title { get; set; } = String.Empty;
        public virtual Guid? Parent { get; set; }
        public virtual ClassificationEntityType Type { get; set; }
        public virtual int? Value { get; set; }
        public virtual int? Index { get; set; }
        public virtual Guid? ClassificationId { get; set; }
        public virtual Guid? UserId { get; set; }

    }
}

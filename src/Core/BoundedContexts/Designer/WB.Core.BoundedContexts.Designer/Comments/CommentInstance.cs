using System;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public class CommentInstance
    {
        public virtual Guid Id { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual Guid EntityId { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string Comment { get; set; }  = String.Empty;
        public virtual string UserName { get; set; } = String.Empty;
        public virtual string UserEmail { get; set; } = String.Empty;
        public virtual DateTime? ResolveDate { get; set; }
    }
}

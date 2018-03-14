using System;
using WB.Core.BoundedContexts.Designer.Verifier;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public class CommentThread
    {
        public CommentView[] Comments { get; set; }
        public QuestionnaireEntityExtendedReference Entity { get; set; }
    }

    public class CommentView
    {
        public Guid Id { get; set; }
        public string EntityId  { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime? ResolveDate { get; set; }
    }
}

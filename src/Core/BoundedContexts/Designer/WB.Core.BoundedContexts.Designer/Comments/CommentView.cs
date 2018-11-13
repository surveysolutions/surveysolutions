using System;
using System.Linq;
using CsQuery.ExtensionMethods;
using WB.Core.BoundedContexts.Designer.Verifier;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public class CommentThread
    {
        public CommentThread(CommentView[] comments, QuestionnaireEntityExtendedReference referenceEntity)
        {
            this.Comments = comments;
            this.Entity = referenceEntity;
            var lastUnresolvedComment = comments.LastOrDefault(x => x.ResolveDate == null);
            if (lastUnresolvedComment != null)
            {
                IndexOfLastUnresolvedComment = comments.IndexOf(lastUnresolvedComment) + 1;
            }
        }

        public CommentView[] Comments { get; set; }
        public QuestionnaireEntityExtendedReference Entity { get; set; }
        public int? IndexOfLastUnresolvedComment { get; set; }
    }

    public class CommentView
    {
        public string Id { get; set; }
        public string EntityId  { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime? ResolveDate { get; set; }
    }
}

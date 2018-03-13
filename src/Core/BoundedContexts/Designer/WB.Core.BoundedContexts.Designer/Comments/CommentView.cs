using System;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public class CommentView
    {
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public DateTime? ResolveDate { get; internal set; }
    }
}

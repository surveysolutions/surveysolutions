using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public interface ICommentsService
    {
        List<CommentView> LoadCommentsForEntity(Guid questionnaireId, Guid entityId);
        void PostComment(AddCommentModel comment);
        void ResolveComment(Guid commentdId);
    }
}

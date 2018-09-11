using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public interface ICommentsService
    {
        List<CommentView> LoadCommentsForEntity(Guid questionnaireId, Guid entityId);
        void PostComment(Guid commentId, Guid questionnaireId, Guid entityId, string commentComment, string userName, string userEmail);
        void ResolveComment(Guid commentdId);
        List<CommentThread> LoadCommentThreads(Guid questionnaireId);

        void RemoveAllCommentsByEntity(Guid questionnaireId, Guid entityId);
        void DeleteAllByQuestionnaireId(Guid questionnaireId);
        void DeleteComment(Guid id);
    }
}

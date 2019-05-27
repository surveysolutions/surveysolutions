using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public interface ICommentsService
    {
        Task<List<CommentView>> LoadCommentsForEntity(Guid questionnaireId, Guid entityId);
        void PostComment(Guid commentId, Guid questionnaireId, Guid entityId, string commentComment, string userName, string userEmail);
        Task ResolveCommentAsync(Guid commentId);
        List<CommentThread> LoadCommentThreads(Guid questionnaireId);

        void RemoveAllCommentsByEntity(Guid questionnaireId, Guid entityId);
        void DeleteAllByQuestionnaireId(Guid questionnaireId);
        Task DeleteCommentAsync(Guid id);
    }
}

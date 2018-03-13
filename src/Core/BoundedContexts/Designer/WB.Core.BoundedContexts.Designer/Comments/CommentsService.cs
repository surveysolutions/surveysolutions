using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public class CommentsService : ICommentsService
    {
        private readonly IPlainStorageAccessor<CommentInstance> comments;


        public CommentsService(IPlainStorageAccessor<CommentInstance> comments)
        {
            this.comments = comments;
        }


        public List<CommentView> LoadCommentsForEntity(Guid questionnaireId, Guid entityId)
        {
            var commentForEntity = this.comments
                .Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId && x.EntityId == entityId).ToList())
                .Select(x => new CommentView
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    UserEmail = x.UserEmail,
                    Date =x.Date,
                    Comment = x.Comment,
                    ResolveDate = x.ResolveDate
                })
                .ToList();

            return commentForEntity;
        }

        public void PostComment(AddCommentModel comment)
        {
            var commentInstanse = new CommentInstance
            {
                Id = comment.Id,
                QuestionnaireId = comment.QuestionnaireId,
                EntityId = comment.EntityId,
                Comment = comment.Comment,
                Date = DateTime.UtcNow,
                ResolveDate = null,
                UserName = comment.UserName,
                UserEmail = comment.UserEmail
            };
            this.comments.Store(commentInstanse, comment.Id);
        }

        public void ResolveComment(Guid commentdId)
        {
            var comment = this.comments.GetById(commentdId);
            comment.ResolveDate = DateTime.UtcNow;
            this.comments.Store(comment, commentdId);
        }
    }
}

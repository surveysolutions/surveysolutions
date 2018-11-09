using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public class CommentsService : ICommentsService
    {
        private readonly IPlainStorageAccessor<CommentInstance> comments;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;

        public CommentsService(
            IPlainStorageAccessor<CommentInstance> comments, 
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.comments = comments;
            this.questionnaireStorage = questionnaireStorage;
        }

        public List<CommentView> LoadCommentsForEntity(Guid questionnaireId, Guid entityId)
        {
            var commentForEntity = this.comments
                .Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId && x.EntityId == entityId).OrderBy(x => x.Date).ToList())
                .Select(CreateCommentView)
                .ToList();

            return commentForEntity;
        }

        private static CommentView CreateCommentView(CommentInstance x)
        {
            return new CommentView
            {
                Id = x.Id.FormatGuid(),
                UserName = x.UserName,
                UserEmail = x.UserEmail,
                Date =x.Date,
                Comment = x.Comment,
                ResolveDate = x.ResolveDate
            };
        }

        public void PostComment(Guid commentId, Guid questionnaireId, Guid entityId, string commentComment, string userName, string userEmail)
        {
            var commentInstanse = new CommentInstance
            {
                Id = commentId,
                QuestionnaireId = questionnaireId,
                EntityId = entityId,
                Comment = commentComment,
                Date = DateTime.UtcNow,
                ResolveDate = null,
                UserName = userName,
                UserEmail = userEmail
            };
            this.comments.Store(commentInstanse, commentId);
        }

        public void RemoveAllCommentsByEntity(Guid questionnaireId, Guid entityId)
        {
            var commentsForEntity = this.comments.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId && x.EntityId == entityId)
                .ToList());

            this.comments.Remove(commentsForEntity);
        }

        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var commentsForEntity = this.comments.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId)
                .ToList());

            this.comments.Remove(commentsForEntity);
        }

        public void DeleteComment(Guid id)
        {
            this.comments.Remove(id);
        }

        public void ResolveComment(Guid commentdId)
        {
            var comment = this.comments.GetById(commentdId);
            comment.ResolveDate = DateTime.UtcNow;
            this.comments.Store(comment, commentdId);
        }

        public List<CommentThread> LoadCommentThreads(Guid questionnaireId)
        {
            ReadOnlyQuestionnaireDocument questionnaire = questionnaireStorage.GetById(questionnaireId.FormatGuid()).AsReadOnly();
            var commentForEntity = this.comments
                .Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId).GroupBy(x => x.EntityId).ToList())
                .Select(x => new CommentThread(
                    comments: x.Select(CreateCommentView).OrderByDescending(c => c.Date).ToArray(), 
                    referenceEntity: CreateCommentedEntity(questionnaire, x.Key)))
                .Where(y => y.Entity != null)
                .ToList();

            return commentForEntity.OrderByDescending(x => x.IndexOfLastUnresolvedComment).ToList();
        }

        private QuestionnaireEntityExtendedReference CreateCommentedEntity(ReadOnlyQuestionnaireDocument questionnaire, Guid itemId)
        {
            var entity = questionnaire.Find<IComposite>(itemId);
            if (entity == null)
                return null;
            var reference = QuestionnaireEntityReference.CreateFrom(entity)?.ExtendedReference(questionnaire);
            return reference;
        }
    }
}

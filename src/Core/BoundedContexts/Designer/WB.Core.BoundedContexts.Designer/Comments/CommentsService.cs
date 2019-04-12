using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public class CommentsService : ICommentsService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;

        public CommentsService(
            DesignerDbContext dbContex, 
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.dbContext = dbContex;
            this.questionnaireStorage = questionnaireStorage;
        }

        public List<CommentView> LoadCommentsForEntity(Guid questionnaireId, Guid entityId)
        {
            var commentForEntity = this.dbContext.CommentInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.EntityId == entityId).OrderBy(x => x.Date)
                .ToList()
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
            this.dbContext.CommentInstances.Add(commentInstanse);
            this.dbContext.SaveChanges();
        }

        public void RemoveAllCommentsByEntity(Guid questionnaireId, Guid entityId)
        {
            var commentsForEntity = this.dbContext.CommentInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.EntityId == entityId)
                .ToList();

            this.dbContext.CommentInstances.RemoveRange(commentsForEntity);
            this.dbContext.SaveChanges();
        }

        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var commentsForEntity = this.dbContext.CommentInstances
                .Where(x => x.QuestionnaireId == questionnaireId)
                .ToList();

            this.dbContext.CommentInstances.RemoveRange(commentsForEntity);
            this.dbContext.SaveChanges();
        }

        public void DeleteComment(Guid id)
        {
            var commentInstance = this.dbContext.CommentInstances.Find(id);

            if (commentInstance != null)
            {
                this.dbContext.CommentInstances.Remove(commentInstance);
                this.dbContext.SaveChanges();
            }
        }

        public void ResolveComment(Guid commentdId)
        {
            var comment = this.dbContext.CommentInstances.Find(commentdId);
            comment.ResolveDate = DateTime.UtcNow;
            this.dbContext.CommentInstances.Update(comment);
            this.dbContext.SaveChanges();
        }

        public List<CommentThread> LoadCommentThreads(Guid questionnaireId)
        {
            ReadOnlyQuestionnaireDocument questionnaire = questionnaireStorage.GetById(questionnaireId.FormatGuid()).AsReadOnly();
            var commentForEntity = this.dbContext.CommentInstances
                .Where(x => x.QuestionnaireId == questionnaireId).GroupBy(x => x.EntityId).ToList()
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

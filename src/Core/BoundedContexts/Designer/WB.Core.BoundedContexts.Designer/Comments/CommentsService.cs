using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
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
            DesignerDbContext dbContext,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.dbContext = dbContext;
            this.questionnaireStorage = questionnaireStorage;
        }

        public async Task<List<CommentView>> LoadCommentsForEntity(Guid questionnaireId, Guid entityId)
        {
            var dbInstances = await dbContext.CommentInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.EntityId == entityId).OrderBy(x => x.Date)
                .ToListAsync();
            var commentForEntity = dbInstances
                .Select(CreateCommentView)
                .ToList();

            return commentForEntity;
        }

        public void PostComment(Guid commentId, Guid questionnaireId, Guid entityId, string commentComment,
            string userName, string userEmail)
        {
            var commentInstance = new CommentInstance
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
            dbContext.CommentInstances.Add(commentInstance);
        }

        public void RemoveAllCommentsByEntity(Guid questionnaireId, Guid entityId)
        {
            var commentsForEntity = dbContext.CommentInstances
                .Where(x => x.QuestionnaireId == questionnaireId && x.EntityId == entityId)
                .ToList();

            dbContext.CommentInstances.RemoveRange(commentsForEntity);
        }

        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var commentsForEntity = dbContext.CommentInstances
                .Where(x => x.QuestionnaireId == questionnaireId)
                .ToList();

            dbContext.CommentInstances.RemoveRange(commentsForEntity);
        }

        public async Task DeleteCommentAsync(Guid id)
        {
            var commentInstance = await dbContext.CommentInstances.FindAsync(id);

            if (commentInstance != null) dbContext.CommentInstances.Remove(commentInstance);
        }

        public async Task ResolveCommentAsync(Guid commentId)
        {
            var comment = await dbContext.CommentInstances.FindAsync(commentId);
            comment.ResolveDate = DateTime.UtcNow;
            dbContext.CommentInstances.Update(comment);
        }

        public List<CommentThread> LoadCommentThreads(Guid questionnaireId)
        {
            var questionnaire = questionnaireStorage.GetById(questionnaireId.FormatGuid())?.AsReadOnly();
            if(questionnaire == null)
                return new List<CommentThread>();

            var commentForEntity = dbContext.CommentInstances.AsNoTracking()
                .Where(x => x.QuestionnaireId == questionnaireId).ToList().GroupBy(x => x.EntityId)
                .Select(x => new CommentThread(
                    x.Select(CreateCommentView).OrderByDescending(c => c.Date).ToArray(),
                    CreateQuestionnaireEntityExtendedReference(questionnaire, x.Key)))
                .Where(y => y.Entity != null)
                .ToList();

            return commentForEntity.OrderByDescending(x => x.IndexOfLastUnresolvedComment).ToList();
        }

        private QuestionnaireEntityExtendedReference? CreateQuestionnaireEntityExtendedReference(ReadOnlyQuestionnaireDocument questionnaire, Guid itemId)
        {
            var entity = questionnaire.Find<IComposite>(itemId);
            if (entity == null)
                return null;
            var reference = QuestionnaireEntityReference.CreateFrom(entity)?.ExtendedReference(questionnaire);
            return reference;
        }

        private static CommentView CreateCommentView(CommentInstance x)
        {
            return new CommentView
            {
                Id = x.Id.FormatGuid(),
                UserName = x.UserName,
                UserEmail = x.UserEmail,
                Date = x.Date,
                Comment = x.Comment,
                ResolveDate = x.ResolveDate
            };
        }
    }
}

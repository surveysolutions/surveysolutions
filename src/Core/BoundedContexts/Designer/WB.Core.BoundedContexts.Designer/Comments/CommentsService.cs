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
                .Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId && x.EntityId == entityId).ToList())
                .Select(CreateCommentView)
                .ToList();

            return commentForEntity;
        }

        private static CommentView CreateCommentView(CommentInstance x)
        {
            return new CommentView
            {
                Id = x.Id,
                UserName = x.UserName,
                UserEmail = x.UserEmail,
                Date =x.Date,
                Comment = x.Comment,
                ResolveDate = x.ResolveDate
            };
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

        public List<CommentThread> LoadCommentThreads(Guid questionnaireId)
        {
            ReadOnlyQuestionnaireDocument questionnaire = questionnaireStorage.GetById(questionnaireId.FormatGuid()).AsReadOnly();
            var commentForEntity = this.comments
                .Query(_ => _.Where(x => x.QuestionnaireId == questionnaireId).GroupBy(x => x.EntityId).ToList())
                .Select(x => new CommentThread
                {
                    Comments = x.Select(CreateCommentView).ToArray(),
                    Entity = CreateCommentedEntity(questionnaire, x.Key)
                })
                .ToList();

            return commentForEntity;
        }

        private QuestionnaireEntityExtendedReference CreateCommentedEntity(ReadOnlyQuestionnaireDocument questionnaire, Guid itemId)
        {
            var entity = questionnaire.Find<IComposite>(itemId);
            var reference = QuestionnaireEntityReference.CreateFrom(entity)?.ExtendedReference(questionnaire);
            return reference;
        }
    }
}

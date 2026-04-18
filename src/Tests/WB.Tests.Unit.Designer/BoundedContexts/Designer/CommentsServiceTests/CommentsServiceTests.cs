using System;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CommentsServiceTests
{
    [TestFixture]
    internal class CommentsServiceTests
    {
        private static CommentsService CreateService(out WB.Core.BoundedContexts.Designer.DataAccess.DesignerDbContext dbContext)
        {
            dbContext = Create.InMemoryDbContext();
            return new CommentsService(dbContext, Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>());
        }

        private static CommentInstance SeedComment(
            WB.Core.BoundedContexts.Designer.DataAccess.DesignerDbContext dbContext,
            Guid? commentId = null,
            Guid? questionnaireId = null)
        {
            var instance = new CommentInstance
            {
                Id = commentId ?? Guid.NewGuid(),
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                Comment = "test comment",
                UserName = "user",
                UserEmail = "user@test.com"
            };
            dbContext.CommentInstances.Add(instance);
            dbContext.SaveChanges();
            return instance;
        }

        // ── DeleteCommentAsync ──────────────────────────────────────────────

        [Test]
        public async Task DeleteCommentAsync_when_comment_belongs_to_questionnaire_then_it_is_removed()
        {
            var service = CreateService(out var db);
            var comment = SeedComment(db);

            await service.DeleteCommentAsync(comment.Id, comment.QuestionnaireId);
            await db.SaveChangesAsync();

            var found = await db.CommentInstances.FindAsync(comment.Id);
            ClassicAssert.IsNull(found);
        }

        [Test]
        public async Task DeleteCommentAsync_when_comment_belongs_to_different_questionnaire_then_it_is_not_removed()
        {
            var service = CreateService(out var db);
            var comment = SeedComment(db);
            var otherQuestionnaireId = Guid.NewGuid(); // attacker supplies their own questionnaire id

            await service.DeleteCommentAsync(comment.Id, otherQuestionnaireId);
            await db.SaveChangesAsync();

            var found = await db.CommentInstances.FindAsync(comment.Id);
            ClassicAssert.IsNotNull(found, "Comment from another questionnaire must not be deleted");
        }

        [Test]
        public async Task DeleteCommentAsync_when_comment_does_not_exist_then_no_exception_is_thrown()
        {
            var service = CreateService(out _);
            var questionnaireId = Guid.NewGuid();

            Assert.DoesNotThrowAsync(() => service.DeleteCommentAsync(Guid.NewGuid(), questionnaireId));
        }

        // ── ResolveCommentAsync ─────────────────────────────────────────────

        [Test]
        public async Task ResolveCommentAsync_when_comment_belongs_to_questionnaire_then_resolve_date_is_set()
        {
            var service = CreateService(out var db);
            var comment = SeedComment(db);

            await service.ResolveCommentAsync(comment.Id, comment.QuestionnaireId);
            await db.SaveChangesAsync();

            var resolved = await db.CommentInstances.FindAsync(comment.Id);
            ClassicAssert.IsNotNull(resolved!.ResolveDate, "ResolveDate should be set");
        }

        [Test]
        public void ResolveCommentAsync_when_comment_belongs_to_different_questionnaire_then_throws()
        {
            var service = CreateService(out var db);
            var comment = SeedComment(db);
            var otherQuestionnaireId = Guid.NewGuid();

            Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ResolveCommentAsync(comment.Id, otherQuestionnaireId));
        }

        [Test]
        public void ResolveCommentAsync_when_comment_does_not_exist_then_throws()
        {
            var service = CreateService(out _);

            Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ResolveCommentAsync(Guid.NewGuid(), Guid.NewGuid()));
        }
    }
}


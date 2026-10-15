#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Tests.Integration.Designer
{
    [TestOf(typeof(AnonymousQuestionnaireStateService))]
    [NonParallelizable]
    public class AnonymousQuestionnaireStateServiceTests : IntegrationTest
    {
        [Test]
        public async Task when_history_staging_fails_once_should_retry_on_postgresql_and_commit_single_row()
        {
            var questionnaireId = Guid.NewGuid();
            var responsibleId = Guid.NewGuid();
            var questionnaireTitle = "Questionnaire title";
            var dbContext = this.ServiceLocator.GetInstance<DesignerDbContext>();
            dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, questionnaireTitle));
            dbContext.SaveChanges();

            var historyMutationService = new ThrowDuplicateKeyOnceHistoryMutationService();
            var service = new AnonymousQuestionnaireStateService(dbContext, historyMutationService);

            await service.SaveStateAsync(questionnaireId, true, questionnaireTitle, responsibleId, "designer-user");

            Assert.That(historyMutationService.CallCount, Is.EqualTo(2));
            Assert.That(dbContext.AnonymousQuestionnaires.Count(x => x.QuestionnaireId == questionnaireId), Is.EqualTo(1));
            Assert.That(dbContext.AnonymousQuestionnaires.Single(x => x.QuestionnaireId == questionnaireId).IsActive, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task when_transaction_is_already_active_should_leave_commit_or_rollback_to_caller(bool commit)
        {
            var questionnaireId = Guid.NewGuid();
            var dbContext = this.ServiceLocator.GetInstance<DesignerDbContext>();
            dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, "Questionnaire title"));
            await dbContext.SaveChangesAsync();

            var historyMutationService = new ThrowDuplicateKeyOnceHistoryMutationService { FailOnFirstCall = false };
            var service = new AnonymousQuestionnaireStateService(dbContext, historyMutationService);

            await using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                await service.SaveStateAsync(questionnaireId, true, "Questionnaire title", Guid.NewGuid(), "designer-user");

                Assert.That(dbContext.Database.CurrentTransaction, Is.SameAs(transaction));
                Assert.That(historyMutationService.CallCount, Is.EqualTo(1));

                if (commit)
                    await transaction.CommitAsync();
                else
                    await transaction.RollbackAsync();
            }

            dbContext.ChangeTracker.Clear();
            Assert.That(await dbContext.AnonymousQuestionnaires.AnyAsync(x => x.QuestionnaireId == questionnaireId),
                Is.EqualTo(commit));
        }

        [Test]
        public async Task when_history_staging_fails_in_callers_transaction_should_not_retry_or_rollback_it()
        {
            var questionnaireId = Guid.NewGuid();
            var dbContext = this.ServiceLocator.GetInstance<DesignerDbContext>();
            dbContext.Questionnaires.Add(Create.Questionnaire.ListViewItem(questionnaireId, "Questionnaire title"));
            await dbContext.SaveChangesAsync();

            var historyMutationService = new ThrowDuplicateKeyOnceHistoryMutationService();
            var service = new AnonymousQuestionnaireStateService(dbContext, historyMutationService);

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            Assert.ThrowsAsync<DbUpdateException>(() =>
                service.SaveStateAsync(questionnaireId, true, "Questionnaire title", Guid.NewGuid(), "designer-user"));

            Assert.That(dbContext.Database.CurrentTransaction, Is.SameAs(transaction));
            Assert.That(historyMutationService.CallCount, Is.EqualTo(1));
            await transaction.RollbackAsync();
        }

        private class ThrowDuplicateKeyOnceHistoryMutationService : IQuestionnaireHistoryMutationService
        {
            public int CallCount { get; private set; }
            public bool FailOnFirstCall { get; set; } = true;

            public Task StageQuestionnaireChangeItemAsync(
                Guid questionnaireId,
                Guid responsibleId,
                string? userName,
                QuestionnaireActionType actionType,
                QuestionnaireItemType targetType,
                Guid targetId,
                string? targetTitle,
                string? targetNewTitle,
                int? affectedEntries,
                DateTime? targetDateTime,
                Main.Core.Documents.QuestionnaireDocument? questionnaireDocument,
                QuestionnaireChangeReference? reference = null,
                QuestionnaireChangeRecordMetadata? meta = null)
            {
                this.CallCount++;
                if (this.FailOnFirstCall && this.CallCount == 1)
                {
                    throw new DbUpdateException("duplicate key", new PostgresException("", "", "", "23505"));
                }

                return Task.CompletedTask;
            }
        }
    }
}

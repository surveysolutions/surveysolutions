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

        private class ThrowDuplicateKeyOnceHistoryMutationService : IQuestionnaireHistoryMutationService
        {
            public int CallCount { get; private set; }

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
                if (this.CallCount == 1)
                {
                    throw new DbUpdateException("duplicate key", new PostgresException("", "", "", "23505"));
                }

                return Task.CompletedTask;
            }
        }
    }
}

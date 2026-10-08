using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class AnonymousQuestionnaireStateService : IAnonymousQuestionnaireStateService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireHistoryMutationService questionnaireHistoryMutationService;

        public AnonymousQuestionnaireStateService(
            DesignerDbContext dbContext,
            IQuestionnaireHistoryMutationService questionnaireHistoryMutationService)
        {
            this.dbContext = dbContext;
            this.questionnaireHistoryMutationService = questionnaireHistoryMutationService;
        }

        public async Task<AnonymousQuestionnaire> SaveStateAsync(
            Guid questionnaireId,
            bool isActive,
            string questionnaireTitle,
            Guid responsibleId,
            string responsibleName)
        {
            for (var attempt = 0; attempt < 2; attempt++)
            {
                await using var transaction =
                    this.dbContext.Database.CurrentTransaction == null
                        ? await this.dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable)
                        : null;

                try
                {
                    if (this.dbContext.Database.IsNpgsql())
                    {
                        var questionnaireLockId = BitConverter.ToInt64(questionnaireId.ToByteArray(), 0);
                        await this.dbContext.Database.ExecuteSqlInterpolatedAsync(
                            $"select pg_advisory_xact_lock({questionnaireLockId})");
                    }

                    var anonymousQuestionnaire = await this.dbContext.AnonymousQuestionnaires
                        .SingleOrDefaultAsync(a => a.QuestionnaireId == questionnaireId);
                    var shouldAddHistoryRecord = anonymousQuestionnaire == null || anonymousQuestionnaire.IsActive != isActive;

                    if (anonymousQuestionnaire == null)
                    {
                        anonymousQuestionnaire = new AnonymousQuestionnaire
                        {
                            QuestionnaireId = questionnaireId,
                            AnonymousQuestionnaireId = Guid.NewGuid(),
                            IsActive = isActive,
                            GeneratedAtUtc = DateTime.UtcNow
                        };
                        this.dbContext.AnonymousQuestionnaires.Add(anonymousQuestionnaire);
                    }
                    else
                    {
                        anonymousQuestionnaire.IsActive = isActive;
                        this.dbContext.AnonymousQuestionnaires.Update(anonymousQuestionnaire);
                    }

                    anonymousQuestionnaire.IsActive = isActive;

                    await this.dbContext.SaveChangesAsync();

                    if (shouldAddHistoryRecord)
                    {
                        var actionType = isActive
                            ? QuestionnaireActionType.AnonymousSharingEnabled
                            : QuestionnaireActionType.AnonymousSharingDisabled;

                        await this.questionnaireHistoryMutationService.StageQuestionnaireChangeItemAsync(
                            questionnaireId,
                            responsibleId,
                            responsibleName,
                            actionType,
                            QuestionnaireItemType.Questionnaire,
                            questionnaireId,
                            questionnaireTitle,
                            null,
                            null,
                            null,
                            null);
                        await this.dbContext.SaveChangesAsync();
                    }

                    if (transaction != null)
                        await transaction.CommitAsync();

                    return anonymousQuestionnaire;
                }
                catch (DbUpdateException exception)
                    when (transaction != null && attempt == 0
                          && exception.InnerException is PostgresException
                          {
                              SqlState: "40001" or "23505"
                          })
                {
                    await transaction.RollbackAsync();
                    this.dbContext.ChangeTracker.Clear();
                }
                catch (PostgresException exception)
                    when (transaction != null && attempt == 0 && exception.SqlState is "40001" or "23505")
                {
                    await transaction.RollbackAsync();
                    this.dbContext.ChangeTracker.Clear();
                }
            }

            throw new InvalidOperationException("Anonymous sharing state update retry limit exceeded.");
        }
    }
}

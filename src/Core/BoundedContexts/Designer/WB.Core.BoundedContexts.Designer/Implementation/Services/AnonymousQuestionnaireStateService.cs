using System;
using System.Data;
using System.Linq;
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
            return await SaveStateAsync(
                questionnaireId,
                isActive,
                questionnaireTitle,
                responsibleId,
                responsibleName,
                regenerateLink: false);
        }

        public async Task<AnonymousQuestionnaire> RegenerateLinkAsync(
            Guid questionnaireId,
            string questionnaireTitle,
            Guid responsibleId,
            string responsibleName)
        {
            return await SaveStateAsync(
                questionnaireId,
                isActive: true,
                questionnaireTitle,
                responsibleId,
                responsibleName,
                regenerateLink: true);
        }

        private async Task<AnonymousQuestionnaire> SaveStateAsync(
            Guid questionnaireId,
            bool isActive,
            string questionnaireTitle,
            Guid responsibleId,
            string responsibleName,
            bool regenerateLink)
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

                    var anonymousQuestionnaires = await this.dbContext.AnonymousQuestionnaires
                        .Where(a => a.QuestionnaireId == questionnaireId)
                        .OrderByDescending(a => a.IsActive)
                        .ThenByDescending(a => a.GeneratedAtUtc)
                        .ThenBy(a => a.AnonymousQuestionnaireId)
                        .ToListAsync();
                    var anonymousQuestionnaire = anonymousQuestionnaires.FirstOrDefault();
                    if (anonymousQuestionnaires.Count > 1)
                        this.dbContext.AnonymousQuestionnaires.RemoveRange(anonymousQuestionnaires.Skip(1));

                    var shouldAddHistoryRecord = (anonymousQuestionnaire?.IsActive ?? false) != isActive;

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
                    else if (regenerateLink)
                    {
                        this.dbContext.AnonymousQuestionnaires.Remove(anonymousQuestionnaire);
                        anonymousQuestionnaire = new AnonymousQuestionnaire
                        {
                            QuestionnaireId = questionnaireId,
                            AnonymousQuestionnaireId = Guid.NewGuid(),
                            IsActive = true,
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

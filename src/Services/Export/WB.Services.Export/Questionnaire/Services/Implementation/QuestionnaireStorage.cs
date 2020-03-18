using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;

namespace WB.Services.Export.Questionnaire.Services.Implementation
{
    internal class QuestionnaireStorage : IQuestionnaireStorage
    {
        private readonly ILogger<QuestionnaireStorage> logger;
        private readonly IQuestionnaireStorageCache cache;
        private readonly ITenantContext tenantContext;

        public QuestionnaireStorage(
            IQuestionnaireStorageCache cache,
            ITenantContext tenantContext,
            ILogger<QuestionnaireStorage> logger)
        {
            this.logger = logger;
            this.cache = cache;
            this.tenantContext = tenantContext;
        }

        private static readonly SemaphoreSlim CacheLock = new SemaphoreSlim(1);

        public async Task<QuestionnaireDocument> GetQuestionnaireAsync(
            QuestionnaireId questionnaireId,
            CancellationToken token = default)
        {
            if (cache.TryGetValue(questionnaireId, out var result))
            {
                return result;
            }

            await CacheLock.WaitAsync(token);

            try
            {
                if (cache.TryGetValue(questionnaireId, out result))
                {
                    return result;
                }

                var questionnaire = await this.tenantContext.Api.GetQuestionnaireAsync(questionnaireId, token);

                if (questionnaire == null) return null;
                questionnaire.QuestionnaireId = questionnaireId;

                foreach (var category in questionnaire.Categories)
                {
                    category.Values = await this.tenantContext.Api.GetCategoriesAsync(questionnaireId, category.Id, token);
                }

                logger.LogDebug("Got questionnaire document from tenant: {tenantName}. {questionnaireId} [{tableName}]",
                    this.tenantContext.Tenant.Name, questionnaire.QuestionnaireId, questionnaire.TableName);

                cache.Set(questionnaireId, questionnaire);

                return questionnaire;
            }
            finally
            {
                CacheLock.Release();
            }
        }

        public void InvalidateQuestionnaire(QuestionnaireId questionnaireId)
        {
            if (cache.TryGetValue(questionnaireId, out var questionnaire) && !questionnaire.IsDeleted)
            {
                cache.Remove(questionnaireId);
            }
        }
    }
}

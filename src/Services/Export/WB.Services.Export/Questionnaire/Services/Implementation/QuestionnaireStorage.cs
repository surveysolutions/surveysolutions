using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Services.Export.Infrastructure;
using WB.ServicesIntegration.Export;

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

        public async Task<QuestionnaireDocument?> GetQuestionnaireAsync(
            QuestionnaireIdentity questionnaireId,
            Guid? translation = null,
            CancellationToken token = default)
        {
            if (cache.TryGetValue(questionnaireId, translation, out var result))
            {
                return result;
            }

            await CacheLock.WaitAsync(token);

            try
            {
                if (cache.TryGetValue(questionnaireId, translation, out result))
                {
                    return result;
                }

                string response = await this.tenantContext.Api.GetQuestionnaireAsync(questionnaireId, translation, token);

                var questionnaire = JsonConvert.DeserializeObject<QuestionnaireDocument>(response);
                
                if (questionnaire == null) return null;
                questionnaire.QuestionnaireId = questionnaireId;

                if (!questionnaire.IsDeleted)
                {
                    logger.LogDebug("Downloading questionnaire categories for questionnaire: {tenantName}. {questionnaireId} [{tableName}]",
                        this.tenantContext.Tenant.Name, questionnaire.QuestionnaireId, questionnaire.TableName);

                    foreach (var category in questionnaire.Categories)
                    {
                        category.Values = await this.tenantContext.Api.GetCategoriesAsync(questionnaireId, category.Id, translation, token);
                    }
                }

                logger.LogDebug("Got questionnaire document from tenant: {tenantName}. {questionnaireId} [{tableName}]",
                    this.tenantContext.Tenant.Name, questionnaire.QuestionnaireId, questionnaire.TableName);

                cache.Set(questionnaireId, translation, questionnaire);

                return questionnaire;
            }
            catch (TaskCanceledException tce)
            {
                if(tce.CancellationToken.IsCancellationRequested)
                    logger.LogWarning("Task was canceled on getting questionnaire: {tenantName}. {questionnaireId}",
                        this.tenantContext.Tenant.Name, questionnaireId);
                throw;
            }
            finally
            {
                CacheLock.Release();
            }
        }

        public void InvalidateQuestionnaire(QuestionnaireIdentity questionnaireId, Guid? translation)
        {
            if (cache.TryGetValue(questionnaireId, translation, out var questionnaire) && !questionnaire!.IsDeleted)
            {
                cache.Remove(questionnaireId, translation);
            }
        }
    }
}

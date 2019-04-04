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
        private readonly IDatabaseSchemaService databaseSchemaService;
        private readonly IQuestionnaireStorageCache cache;
        private readonly JsonSerializerSettings serializer;
        private readonly ITenantContext tenantContext;

        public QuestionnaireStorage(
            IQuestionnaireStorageCache cache,
            IDatabaseSchemaService databaseSchemaService,
            ITenantContext tenantContext,
            ILogger<QuestionnaireStorage> logger)
        {
            this.logger = logger;
            this.cache = cache;
            this.databaseSchemaService = databaseSchemaService;
            this.tenantContext = tenantContext;
            this.serializer = new JsonSerializerSettings
            {
                SerializationBinder = new QuestionnaireDocumentSerializationBinder(),
                TypeNameHandling = TypeNameHandling.Auto
            };
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

                var questionnaire = await this.tenantContext.Api.GetQuestionnaireAsync(questionnaireId);

                if (questionnaire == null) return null;
                questionnaire.QuestionnaireId = questionnaireId;

                logger.LogDebug("Got questionnaire document from tenant: {tenantName}. {questionnaireId} [{tableName}]",
                    this.tenantContext.Tenant.Name, questionnaire.QuestionnaireId, questionnaire.TableName);
                
                cache.Set(questionnaireId, questionnaire);

                if (questionnaire.IsDeleted)
                {
                    if (databaseSchemaService.TryDropQuestionnaireDbStructure(questionnaire))
                    {
                        this.cache.Remove(questionnaireId);
                    }
                }
                else
                {
                    databaseSchemaService.CreateQuestionnaireDbStructure(questionnaire);
                }

                return questionnaire;
            }
            finally
            {
                CacheLock.Release();
            }
        }
    }
}

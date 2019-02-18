using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WB.Services.Export.Events;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Questionnaire.Services.Implementation
{
    internal class QuestionnaireStorage : IQuestionnaireStorage
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly ILogger<QuestionnaireStorage> logger;
        private readonly IMemoryCache memoryCache;
        private readonly IServiceProvider serviceProvider;
        private readonly JsonSerializerSettings serializer;

        public QuestionnaireStorage(ITenantApi<IHeadquartersApi> tenantApi,
            ILogger<QuestionnaireStorage> logger,
            IMemoryCache memoryCache,
            IServiceProvider serviceProvider)
        {
            this.tenantApi = tenantApi;
            this.logger = logger;
            this.memoryCache = memoryCache;
            this.serviceProvider = serviceProvider;
            this.serializer = new JsonSerializerSettings
            {
                SerializationBinder = new QuestionnaireDocumentSerializationBinder(),
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        private static readonly SemaphoreSlim CacheLock = new SemaphoreSlim(1);

        public async Task<QuestionnaireDocument> GetQuestionnaireAsync(TenantInfo tenant, 
            QuestionnaireId questionnaireId, 
            bool forceUpdate = false,
            CancellationToken token = default)
        {
            var key = $"{nameof(QuestionnaireStorage)}:{tenant}:{questionnaireId}";

            var gotFromCache = memoryCache.TryGetValue(key, out var result);

            if (gotFromCache)
            {
                var questionnaireDocument = (QuestionnaireDocument)result;
                if (!questionnaireDocument.IsDeleted && forceUpdate)
                {
                    memoryCache.Remove(key);
                }
                else
                {
                    return questionnaireDocument;
                }
            }

            await CacheLock.WaitAsync(token);

            try
            {
                if (memoryCache.TryGetValue(key, out result))
                {
                    return (QuestionnaireDocument)result;
                }

                var questionnaireDocument = await this.tenantApi.For(tenant).GetQuestionnaireAsync(questionnaireId);
                var questionnaire = JsonConvert.DeserializeObject<QuestionnaireDocument>(questionnaireDocument, serializer);
                
                questionnaire.QuestionnaireId = questionnaireId;

                memoryCache.Set(key, questionnaire, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });

                if (!questionnaire.IsDeleted)
                {
                    using (var scope = this.serviceProvider.CreateScope())
                    {
                        scope.ServiceProvider.SetTenant(tenant);
                        var initializer = scope.ServiceProvider.GetService<IDatabaseSchemaService>();
                        initializer.CreateQuestionnaireDbStructure(questionnaire);

                        var tenantName = tenant.Name;
                        logger.LogInformation("Created database structure for {tenantName} ({questionnaireId})", tenantName, questionnaireId);
                    }
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

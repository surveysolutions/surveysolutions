using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Questionnaire.Services.Implementation
{
    internal class QuestionnaireStorage : IQuestionnaireStorage
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly IMemoryCache memoryCache;
        private readonly JsonSerializerSettings serializer;

        public QuestionnaireStorage(ITenantApi<IHeadquartersApi> tenantApi, IMemoryCache memoryCache)
        {
            this.tenantApi = tenantApi;
            this.memoryCache = memoryCache;
            this.serializer = new JsonSerializerSettings
            {
                SerializationBinder = new QuestionnaireDocumentSerializationBinder(),
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public async Task<QuestionnaireDocument> GetQuestionnaireAsync(TenantInfo tenant, QuestionnaireId questionnaireId)
        {
            return await this.memoryCache.GetOrCreateAsync($"{nameof(QuestionnaireStorage)}:{tenant}:{questionnaireId}",
                async entry =>
                {
                    var questionnaireDocument = await this.tenantApi.For(tenant).GetQuestionnaireAsync(questionnaireId);

                    var questionnaire = JsonConvert.DeserializeObject<QuestionnaireDocument>(questionnaireDocument, serializer);
                    entry.SlidingExpiration = TimeSpan.FromMinutes(1);
                    return questionnaire;
                });

        }
    }
}

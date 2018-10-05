using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Questionnaire.Services.Implementation
{
    internal class QuestionnaireStorage : IQuestionnaireStorage
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly JsonSerializerSettings serializer;

        public QuestionnaireStorage(ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.tenantApi = tenantApi;
            this.serializer = new JsonSerializerSettings
            {
                SerializationBinder = new QuestionnaireDocumentSerializationBinder(),
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public async Task<QuestionnaireDocument> GetQuestionnaireAsync(TenantInfo tenant, QuestionnaireId questionnaireId)
        {
            var questionnaireDocument = await this.tenantApi.For(tenant).GetQuestionnaireAsync(questionnaireId);

            var questionnaire = JsonConvert.DeserializeObject<QuestionnaireDocument>(questionnaireDocument, serializer);

            return questionnaire;
        }
    }
}

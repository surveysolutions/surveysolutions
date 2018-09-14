using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Services.Implementation
{
    internal class HeadquartersApi : IHeadquartersApi
    {
        private readonly HttpClient client;
        private const string ApiKeyUriArgumentName = "apikey";

        public HeadquartersApi()
        {
            client = new HttpClient();
        }

        public async Task<List<InterviewComment>> GetInterviewCommentsAsync(string tenantBaseUrl, TenantId tenantId,
            Guid interviewId)
        {
            var uri = new Uri(new Uri(tenantBaseUrl), $"api/export/v1/interview/{interviewId}/commentaries");
            var fullUrl = uri + $"?{ApiKeyUriArgumentName}={tenantId}";
            var responseString = await client.GetStringAsync(fullUrl);
            return JsonConvert.DeserializeObject<List<InterviewComment>>(responseString);
        }

        public async Task<List<InterviewToExport>> GetInterviewsToExportAsync(string tenantBaseUrl,
            TenantId tenantId,
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var uri = new Uri(new Uri(tenantBaseUrl), $"api/export/v1/interview");
            var fullUrl = uri + $"?{ApiKeyUriArgumentName}={tenantId}&questionnaireIdentity={questionnaireIdentity}&status={status}&fromDate={fromDate}&toDate={toDate}";
            var responseString = await client.GetStringAsync(fullUrl);
            return JsonConvert.DeserializeObject<List<InterviewToExport>>(responseString);
        }

        public async Task<QuestionnaireDocument> GetQuestionnaireAsync(string tenantBaseUrl, TenantId tenantId,
            QuestionnaireId questionnaireId)
        {
            var normalizedBaseUrl = tenantBaseUrl.TrimEnd('/');

            var uri = new Uri($"{normalizedBaseUrl}/api/export/v1/questionnaire/{questionnaireId}");
            var fullUrl = uri + $"?{ApiKeyUriArgumentName}={tenantId}";
            var response = await client.GetAsync(fullUrl);
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<QuestionnaireDocument>(responseString, new JsonSerializerSettings
            {
                SerializationBinder = new QuestionnaireDocumentSerializationBinder()
            });
        }
    }

    internal class QuestionnaireDocumentSerializationBinder : DefaultSerializationBinder
    {
        private static readonly Assembly Assembly = typeof(QuestionnaireDocument).Assembly;
        private static readonly Dictionary<string, Type> ResolvedTypes = new Dictionary<string, Type>();

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (!ResolvedTypes.ContainsKey(typeName))
            {
                var type = Assembly.GetTypes().FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.Ordinal));
                ResolvedTypes[typeName] = type;
            }

            return ResolvedTypes[typeName];
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }
}

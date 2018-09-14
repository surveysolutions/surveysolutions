using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}

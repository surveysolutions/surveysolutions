using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.WebTester.Services
{
    public class WebTesterScenarioItem
    {
        public string Title { get; set; } = "";
        public int Id { get; set; }
    }

    public class SaveScenarioRequest
    {
        public string? ScenarioText { get; set; }
        public int? ScenarioId { get; set; }
        public string? ScenarioTitle { get; set; }
    }

    public interface IDesignerWebTesterApi
    {
        [Get("/api/webtester/{questionnaireId}/info")]
        Task<QuestionnaireLiteInfo> GetQuestionnaireInfoAsync(string questionnaireId);

        [Get("/api/webtester/{questionnaireId}/questionnaire")]
        Task<Questionnaire> GetQuestionnaireAsync(string questionnaireId);

        [Get("/api/webtester/{questionnaireId}/attachment/{attachmentContentId}")]
        Task<AttachmentContent> GetAttachmentContentAsync(string questionnaireId, string attachmentContentId);

        [Get("/api/webtester/{questionnaireId}/translations")]
        Task<List<TranslationDto>> GetTranslationsAsync(string questionnaireId);

        [Get("/api/webtester/{questionnaireId}/categories")]
        Task<List<CategoriesDto>> GetCategoriesAsync(string questionnaireId);

        [Get("/api/webtester/Scenarios/{questionnaireId}/{scenarioId}")]
        Task<ApiResponse<string>> GetScenario(string questionnaireId, int scenarioId);

        [Get("/api/webtester/Scenarios/{questionnaireId}")]
        Task<IApiResponse<List<WebTesterScenarioItem>>> GetScenariosListAsync(string questionnaireId);

        [Post("/api/webtester/Scenarios/{questionnaireId}")]
        Task<IApiResponse> SaveScenarioAsync(string questionnaireId, [Body] SaveScenarioRequest model);

        [Get("/.hc")]
        Task<string> HealthCheck();

        [Get("/api/webtester/{questionnaireId}/settings")]
        Task<QuestionnaireSettings> GetQuestionnaireSettingsAsync(string questionnaireId);

        /// <summary>
        /// Backend-to-backend: exchanges a one-time code for a delegated JWT.
        /// Authenticated via X-Service-Name / X-Service-Key headers.
        /// </summary>
        [Post("/api/internal/auth/exchange")]
        Task<ExchangeCodeResponse> ExchangeCodeAsync(
            [Body] ExchangeCodeRequest request,
            [Header("X-Service-Name")] string serviceName,
            [Header("X-Service-Key")] string serviceKey,
            CancellationToken cancellationToken = default);
    }
}

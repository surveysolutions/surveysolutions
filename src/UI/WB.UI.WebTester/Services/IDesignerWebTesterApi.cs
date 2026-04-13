using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.WebTester.Services
{
    public interface IDesignerWebTesterApi
    {
        [Get("/api/webtester/{questionnaireId}/info")]
        Task<QuestionnaireLiteInfo> GetQuestionnaireInfoAsync(string questionnaireId, [Header("Authorization")] string authorization);

        [Get("/api/webtester/{questionnaireId}/questionnaire")]
        Task<Questionnaire> GetQuestionnaireAsync(string questionnaireId, [Header("Authorization")] string authorization);

        [Get("/api/webtester/{questionnaireId}/attachment/{attachmentContentId}")]
        Task<AttachmentContent> GetAttachmentContentAsync(string questionnaireId, string attachmentContentId, [Header("Authorization")] string authorization);

        [Get("/api/webtester/{questionnaireId}/translations")]
        Task<List<TranslationDto>> GetTranslationsAsync(string questionnaireId, [Header("Authorization")] string authorization);

        [Get("/api/webtester/{questionnaireId}/categories")]
        Task<List<CategoriesDto>> GetCategoriesAsync(string questionnaireId, [Header("Authorization")] string authorization);

        [Get("/api/webtester/Scenarios/{questionnaireId}/{scenarioId}")]
        Task<ApiResponse<string>> GetScenario(string questionnaireId, int scenarioId, [Header("Authorization")] string authorization);

        [Get("/.hc")]
        Task<string> HealthCheck();
        
        [Get("/api/webtester/{questionnaireId}/settings")]
        Task<QuestionnaireSettings> GetQuestionnaireSettingsAsync(string questionnaireId, [Header("Authorization")] string authorization);
    }
}

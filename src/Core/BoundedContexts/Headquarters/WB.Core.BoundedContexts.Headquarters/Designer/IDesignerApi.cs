using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Synchronization.Designer;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Headquarters.Designer
{
    public interface IDesignerApi
    {
        [Post("/api/hq/v3/questionnaires/{id}/revision/{rev}/metadata")]
        Task UpdateRevisionMetadata(Guid id, int rev, [Body] QuestionnaireRevisionMetadataModel model);

        [Get("/api/hq/v3/questionnaires/{id}")]
        Task<QuestionnaireCommunicationPackage> GetQuestionnaire(
            Guid id,
            int clientQuestionnaireContentVersion,
            [Query] int? minSupportedQuestionnaireVersion);

        [Get("/api/hq/attachment/{contentId}")]
        Task<RestFile> DownloadQuestionnaireAttachment(string contentId, [Query] Guid attachmentId);

        [Get("/api/hq/translations/{questionnaireId}")]
        Task<List<TranslationDto>> GetTranslations(Guid questionnaireId);

        [Get("/api/hq/lookup/{questionnaireId}/{lookupId}")]
        Task<QuestionnaireLookupTable> GetLookupTables(Guid questionnaireId, Guid lookupId);

        [Get("/api/hq/v3/questionnaires/info/{questionnaireId}")]
        Task<QuestionnaireInfo> GetQuestionnaireInfo(Guid questionnaireId);

        [Get("/api/hq/user/login")]
        Task<string> Login([Header("Authorization")] string authorization);

        [Get("/api/hq/user/userdetails")]
        Task IsLoggedIn();

        [Get("/api/hq/v3/questionnaires")]
        Task<PagedQuestionnaireCommunicationPackage> GetQuestionnairesList([Query] DesignerQuestionnairesListFilter filter);

        [Get("/pdf/status/{questionnaireId}")]
        Task <PdfStatus> GetPdfStatus(Guid questionnaireId, [Query] Guid? translation = null);

        [Get("/pdf/download/{questionnaireId}")]
        Task<RestFile> DownloadPdf(Guid questionnaireId, [Query] Guid? translation = null);

        [Get("/api/hq/categories/{questionnaireId}/{categoryId}")]
        Task<List<CategoriesItem>> GetReusableCategories(Guid questionnaireId, Guid categoryId);

        [Get("/api/hq/backup/{questionnaireId}")]
        Task<RestFile> DownloadQuestionnaireBackup(Guid questionnaireId);
        
        [Get("/.version")]
        Task<string> Version();
    }

    public class DesignerQuestionnairesListFilter
    {
        [AliasAs("pageIndex")]
        public int PageIndex { get; set; }

        [AliasAs("pageSize")]
        public int PageSize { get; set; }

        [AliasAs("sortOrder")]
        public string SortOrder { get; set; }

        [AliasAs("filter")]
        public string Filter { get; set; }
    }

    public class QuestionnaireRevisionMetadataModel
    {
        public int HqTimeZoneMinutesOffset { get; set; }
        public string HqImporterLogin { get; set; }
        public long HqQuestionnaireVersion { get; set; }
        public string Comment { get; set; }
        public string HqHost { get; set; }
    }
}

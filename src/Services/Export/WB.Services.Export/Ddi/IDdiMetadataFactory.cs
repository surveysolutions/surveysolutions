using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Ddi
{
    public interface IDdiMetadataFactory
    {
        Task<string> CreateDDIMetadataFileForQuestionnaireInFolderAsync(TenantInfo tenant, QuestionnaireId questionnaireId,
            string basePath);
    }
}

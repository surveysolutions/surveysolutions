using System.Threading.Tasks;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Ddi
{
    public interface IDdiMetadataFactory
    {
        Task<string> CreateDDIMetadataFileForQuestionnaireInFolderAsync(TenantInfo tenant, QuestionnaireIdentity questionnaireId,
            string basePath);
    }
}

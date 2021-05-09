using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Ddi
{
    public interface IDdiMetadataAccessor
    {
        Task<string> GetFilePathToDDIMetadataAsync(TenantInfo tenant, QuestionnaireIdentity questionnaireId, string? password);

        void ClearFiles(TenantInfo tenant);
    }
}

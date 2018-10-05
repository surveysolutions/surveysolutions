using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Ddi
{
    public interface IDdiMetadataAccessor
    {
        Task<string> GetFilePathToDDIMetadata(TenantInfo tenant, QuestionnaireId questionnaireId, string password);

        void ClearFiles(TenantInfo tenant);
    }
}

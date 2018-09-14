using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Interview
{
    internal interface IQuestionnaireExportStructureFactory
    {
        Task<QuestionnaireExportStructure> GetQuestionnaireExportStructure(QuestionnaireId questionnaireId,
            string tenantBaseUrl,
            TenantId tenantId);
    }
}

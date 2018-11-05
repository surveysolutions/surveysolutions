using System.Threading.Tasks;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Questionnaire
{
    public interface IQuestionnaireExportStructureFactory
    {
        QuestionnaireExportStructure GetQuestionnaireExportStructure(TenantInfo tenant,
            QuestionnaireDocument questionnaire);

        Task<QuestionnaireExportStructure> GetQuestionnaireExportStructureAsync(TenantInfo tenant,
            QuestionnaireId questionnaireId);
    }
}

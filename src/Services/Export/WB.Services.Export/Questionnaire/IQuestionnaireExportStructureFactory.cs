using System.Threading.Tasks;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Questionnaire
{
    public interface IQuestionnaireExportStructureFactory
    {
        QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire);

        Task<QuestionnaireExportStructure> GetQuestionnaireExportStructureAsync(
            TenantInfo tenant,
            QuestionnaireId questionnaireId);
    }
}

using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Interview
{
    internal interface IQuestionnaireExportStructureFactory
    {
        QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireDocument questionnaire,
            TenantInfo tenant);
    }
}

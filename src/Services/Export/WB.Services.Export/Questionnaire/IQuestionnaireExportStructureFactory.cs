using WB.Services.Export.Tenant;

namespace WB.Services.Export.Questionnaire
{
    internal interface IQuestionnaireExportStructureFactory
    {
        QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireDocument questionnaire,
            TenantInfo tenant);
    }
}

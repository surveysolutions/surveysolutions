using WB.Services.Export.Tenant;

namespace WB.Services.Export.Questionnaire
{
    internal interface IQuestionnaireExportStructureFactory
    {
        QuestionnaireExportStructure GetQuestionnaireExportStructure(TenantInfo tenant,
            QuestionnaireDocument questionnaire);

        QuestionnaireExportStructure GetQuestionnaireExportStructure(TenantInfo tenant,
            QuestionnaireId questionnaireId);
    }
}

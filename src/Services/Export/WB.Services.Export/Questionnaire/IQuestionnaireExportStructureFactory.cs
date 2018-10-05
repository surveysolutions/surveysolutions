using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Questionnaire
{
    public interface IQuestionnaireExportStructureFactory
    {
        QuestionnaireExportStructure GetQuestionnaireExportStructure(TenantInfo tenant,
            QuestionnaireDocument questionnaire);

        QuestionnaireExportStructure GetQuestionnaireExportStructure(TenantInfo tenant,
            QuestionnaireId questionnaireId);
    }
}

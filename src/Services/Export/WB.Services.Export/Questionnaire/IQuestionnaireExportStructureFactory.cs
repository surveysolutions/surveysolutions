using System;
using System.Threading.Tasks;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Questionnaire
{
    public interface IQuestionnaireExportStructureFactory
    {
        QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire);

        Task<QuestionnaireExportStructure> GetQuestionnaireExportStructureAsync(
            TenantInfo tenant,
            QuestionnaireIdentity questionnaireId,
            Guid? translation = null);
    }
}

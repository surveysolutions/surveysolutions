using System.Threading.Tasks;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Questionnaire.Services
{
    internal interface IQuestionnaireStorage
    {
        Task<QuestionnaireDocument> GetQuestionnaireAsync(TenantInfo tenant, QuestionnaireId questionnaireId);
    }
}

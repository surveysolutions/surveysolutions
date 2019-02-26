using System.Threading;
using System.Threading.Tasks;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Questionnaire.Services
{
    public interface IQuestionnaireStorage
    {
        Task<QuestionnaireDocument> GetQuestionnaireAsync(QuestionnaireId questionnaireId,
            CancellationToken token = default);
    }
}

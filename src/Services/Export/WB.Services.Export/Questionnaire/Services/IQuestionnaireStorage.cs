using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Questionnaire.Services
{
    public interface IQuestionnaireStorage
    {
        Task<QuestionnaireDocument> GetQuestionnaireAsync(QuestionnaireId questionnaireId,
            CancellationToken token = default);

        void InvalidateQuestionnaire(QuestionnaireId questionnaireId);
    }
}

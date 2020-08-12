using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Questionnaire.Services
{
    public interface IQuestionnaireStorage
    {
        Task<QuestionnaireDocument?> GetQuestionnaireAsync(QuestionnaireId questionnaireId,
            Guid? translation = null,
            CancellationToken token = default);

        void InvalidateQuestionnaire(QuestionnaireId questionnaireId, Guid? translation = null);
    }
}

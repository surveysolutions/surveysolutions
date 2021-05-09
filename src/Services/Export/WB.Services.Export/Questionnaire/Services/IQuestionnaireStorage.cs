using System;
using System.Threading;
using System.Threading.Tasks;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Questionnaire.Services
{
    public interface IQuestionnaireStorage
    {
        Task<QuestionnaireDocument?> GetQuestionnaireAsync(QuestionnaireIdentity questionnaireId,
            Guid? translation = null,
            CancellationToken token = default);

        void InvalidateQuestionnaire(QuestionnaireIdentity questionnaireId, Guid? translation = null);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IInterviewQuestionnaireReferenceStorage
    {
        Task<QuestionnaireId> GetQuestionnaireIdByInterviewIdAsync(Guid interviewId, CancellationToken cancellationToken);

        Task AddInterviewQuestionnaireReferenceAsync(Guid interviewId, QuestionnaireId questionnaireId, CancellationToken cancellationToken);

        Task RemoveInterviewQuestionnaireReferenceAsync(Guid interviewId, CancellationToken cancellationToken);
    }
}
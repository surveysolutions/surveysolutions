using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAnswerToStringService
    {
        string AnswerToUIString(BaseQuestionModel question, BaseInterviewAnswer answer, IStatefulInterview interview, QuestionnaireModel questionnaire);
    }
}
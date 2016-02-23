using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAnswerToStringService
    {
        string AnswerToUIString(Guid questionId, BaseInterviewAnswer answer, IStatefulInterview interview, IQuestionnaire questionnaire);
    }
}
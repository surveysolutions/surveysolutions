using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAnswerToStringConverter
    {
        string GetUiStringAnswerForIdentifyingQuestionOrThrow(object answer, Guid questionId, IQuestionnaire questionnaire);
        string GetUiStringAnswerForIdentifyingQuestionOrThrow(AbstractAnswer answer, Guid questionId, IQuestionnaire questionnaire);
    }
}

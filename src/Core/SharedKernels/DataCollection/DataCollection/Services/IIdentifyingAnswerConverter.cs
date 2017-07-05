using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IIdentifyingAnswerConverter
    {
        AbstractAnswer GetAbstractAnswer(IQuestionnaire questionnaire, Guid questionId, string answer);
    }
}
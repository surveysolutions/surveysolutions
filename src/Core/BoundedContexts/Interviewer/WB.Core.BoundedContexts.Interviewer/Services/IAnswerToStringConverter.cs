using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IAnswerToStringConverter
    {
        string Convert(object answer, Guid questionId, IQuestionnaire questionnaire);
    }
}
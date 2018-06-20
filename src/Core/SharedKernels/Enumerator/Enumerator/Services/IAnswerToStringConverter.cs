using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAnswerToStringConverter
    {
        string Convert(object answer, Guid questionId, IQuestionnaire questionnaire);
    }
}

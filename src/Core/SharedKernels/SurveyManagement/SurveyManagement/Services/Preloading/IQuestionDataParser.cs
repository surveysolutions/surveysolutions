using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    internal interface IQuestionDataParser
    {
        KeyValuePair<Guid, object>? Parse(string answer, string variableName, Func<string, IQuestion> getQuestionByStataCaption,
            Func<Guid, IEnumerable<decimal>> getAnswerOptionsAsValues);

        KeyValuePair<Guid, object>? BuildAnswerForVariableName(string[] answers, KeyValuePair<string, int[]> columnIndexesForVariableName, Func<string, IQuestion> getQuestionByStataCaption,
            Func<Guid, IEnumerable<decimal>> getAnswerOptionsAsValues);
    }
}

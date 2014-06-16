using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    internal interface IQuestionDataParser
    {
        ValueParsingResult TryParse(string answer, IQuestion question, out KeyValuePair<Guid, object> value);

        KeyValuePair<Guid, object>? BuildAnswerFromStringArray(string[] answers, IQuestion question);
    }
}

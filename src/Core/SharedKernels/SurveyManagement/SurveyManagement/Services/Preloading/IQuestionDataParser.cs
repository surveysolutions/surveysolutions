using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    internal interface IQuestionDataParser
    {
        ValueParsingResult TryParse(string answer, string variableName, QuestionnaireDocument questionnaire, out KeyValuePair<Guid, object> value);

        KeyValuePair<Guid, object>? BuildAnswerFromStringArray(string[] answers, string variableName, QuestionnaireDocument questionnaire);
    }
}

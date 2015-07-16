using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    internal interface IQuestionDataParser
    {
        ValueParsingResult TryParse(string answer, string columnName, IQuestion question, QuestionnaireDocument questionnaire, out KeyValuePair<Guid, object> value);

        KeyValuePair<Guid, object>? BuildAnswerFromStringArray(Tuple<string, string>[] answersWithColumnName, IQuestion question, QuestionnaireDocument questionnaire);
    }
}

using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Preloading
{
    internal interface IQuestionDataParser
    {
        KeyValuePair<Guid, object>? Parse(string answer, string variableName, QuestionnaireDocument questionnaire);

        KeyValuePair<Guid, object>? BuildAnswerFromStringArray(string[] answers, string variableName, QuestionnaireDocument questionnaire);
    }
}

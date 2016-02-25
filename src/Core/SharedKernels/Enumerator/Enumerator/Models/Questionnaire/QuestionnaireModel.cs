using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;

namespace WB.Core.SharedKernels.Enumerator.Models.Questionnaire
{
    [Obsolete("Use IQuestionnaire instead")]
    public class QuestionnaireModel
    {
        public Dictionary<Guid, BaseQuestionModel> Questions { get; set; }
    }
}

using System;

namespace WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions
{
    public class LinkedToRosterMultiOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToRosterId { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public bool AreAnswersOrdered { get; set; }
    }
}
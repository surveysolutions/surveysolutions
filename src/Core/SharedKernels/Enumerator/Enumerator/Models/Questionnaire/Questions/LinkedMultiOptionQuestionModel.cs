using System;

namespace WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions
{
    public class LinkedMultiOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToQuestionId { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public bool AreAnswersOrdered { get; set; }
    }
}
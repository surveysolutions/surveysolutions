using System;

namespace WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions
{
    public class LinkedSingleOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToQuestionId { get; set; }
    }

    public class LinkedToRosterSingleOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToRosterId { get; set; }
    }
}
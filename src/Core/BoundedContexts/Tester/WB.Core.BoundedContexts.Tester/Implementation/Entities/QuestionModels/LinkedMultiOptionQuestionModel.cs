using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels
{
    public class LinkedMultiOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToQuestionId { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public bool AreAnswersOrdered { get; set; }
    }
}
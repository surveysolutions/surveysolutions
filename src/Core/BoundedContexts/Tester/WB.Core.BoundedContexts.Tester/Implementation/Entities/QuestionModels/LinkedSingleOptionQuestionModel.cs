using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels
{
    public class LinkedSingleOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToQuestionId { get; set; }
    }
}
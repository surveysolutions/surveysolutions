using System;

namespace WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels
{
    public class LinkedSingleOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToQuestionId { get; set; }
    }
}
using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels
{
    public class LinkedSingleOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToQuestionId { get; set; }
    }
}
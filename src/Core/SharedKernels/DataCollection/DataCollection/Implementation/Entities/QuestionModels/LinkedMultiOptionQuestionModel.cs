using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels
{
    public class LinkedMultiOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToQuestionId { get; set; }
    }
}
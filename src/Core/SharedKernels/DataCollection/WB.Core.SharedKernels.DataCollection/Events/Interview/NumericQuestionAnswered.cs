using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class NumericQuestionAnswered : QuestionAnswered
    {
        public decimal Answer { get; private set; }

        public NumericQuestionAnswered(Guid userId, Guid questionId, DateTime answerTime, decimal answer)
            : base(userId, questionId, answerTime)
        {
            this.Answer = answer;
        }
    }
}
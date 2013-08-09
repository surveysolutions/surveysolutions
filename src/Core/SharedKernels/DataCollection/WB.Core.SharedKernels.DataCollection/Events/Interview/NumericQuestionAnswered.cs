using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class NumericQuestionAnswered : QuestionAnswered
    {
        public decimal Answer { get; private set; }

        public NumericQuestionAnswered(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, decimal answer)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}
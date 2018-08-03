using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class NumericIntegerQuestionAnswered : QuestionAnswered
    {
        public int Answer { get; private set; }

        public NumericIntegerQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, int answer)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.Answer = answer;
        }
    }
}

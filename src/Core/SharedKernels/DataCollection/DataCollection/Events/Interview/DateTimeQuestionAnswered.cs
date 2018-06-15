using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class DateTimeQuestionAnswered : QuestionAnswered
    {
        public DateTime Answer { get; private set; }

        public DateTimeQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, DateTime answer)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.Answer = answer;
        }
    }
}

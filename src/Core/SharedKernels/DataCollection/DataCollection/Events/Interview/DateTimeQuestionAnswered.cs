using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class DateTimeQuestionAnswered : QuestionAnswered
    {
        public DateTime Answer { get; private set; }

        public DateTimeQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTimeUtc, DateTime answer)
            : base(userId, questionId, rosterVector, answerTimeUtc)
        {
            this.Answer = answer;
        }
    }
}
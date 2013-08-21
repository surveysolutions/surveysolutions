using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class DateTimeQuestionAnswered : QuestionAnswered
    {
        public DateTime Answer { get; private set; }

        public DateTimeQuestionAnswered(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, DateTime answer)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}
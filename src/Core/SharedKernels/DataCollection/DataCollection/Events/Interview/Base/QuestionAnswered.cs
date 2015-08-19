using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionAnswered : QuestionActiveEvent
    {
        public DateTime AnswerTimeUtc { get; private set; }

        protected QuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTimeUtc)
            : base(userId, questionId, rosterVector)
        {
            this.AnswerTimeUtc = answerTimeUtc;
        }
    }
}
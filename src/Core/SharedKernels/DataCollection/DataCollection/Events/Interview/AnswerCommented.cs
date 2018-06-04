using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerCommented : QuestionActiveEvent
    {
        public DateTime? CommentTime { get; private set; }

        public string Comment { get; private set; }

        public AnswerCommented(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate, string comment, DateTime? commentTime = null)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.Comment = comment;

            if (originDate != default(DateTimeOffset))
            {
                this.CommentTime = originDate.UtcDateTime;
            }
            else if (commentTime != null && commentTime != default(DateTime))
            {
                this.CommentTime = commentTime;
            }
        }
    }
}

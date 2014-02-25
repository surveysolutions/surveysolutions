using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerCommented : QuestionActiveEvent
    {
        public DateTime CommentTime { get; private set; }

        public string Comment { get; private set; }

        public AnswerCommented(Guid userId, Guid questionId, decimal[] propagationVector, DateTime commentTime, string comment)
            : base(userId, questionId, propagationVector)
        {
            this.Comment = comment;
            this.CommentTime = commentTime;
        }
    }
}
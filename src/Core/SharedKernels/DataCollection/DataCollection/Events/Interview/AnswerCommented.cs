using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerCommented : QuestionActiveEvent
    {
        public DateTime? CommentTime { get; set; }

        public string Comment { get; private set; }

        public AnswerCommented(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate, string comment)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.Comment = comment;
        }
    }
}

using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerCommented : QuestionActiveEvent
    {
        [Obsolete("Please use OriginDate property")]
        public DateTime? CommentTime { get; set; }

        public string Comment { get; private set; }

        public Guid? CommentId { get; }

        public AnswerCommented(Guid userId, 
            Guid questionId, 
            decimal[] rosterVector, 
            DateTimeOffset originDate, 
            string comment,
            Guid? commentId)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.Comment = comment;
            CommentId = commentId;
        }
    }
}

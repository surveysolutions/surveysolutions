using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerCommented : QuestionActiveEvent
    {
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

    public class AnswerCommentResolved : QuestionActiveEvent
    {
        public Guid CommentId { get; }

        public AnswerCommentResolved(Guid userId, 
            Guid questionId, 
            RosterVector rosterVector, 
            DateTimeOffset originDate, 
            Guid commentId)
            : base(userId, questionId, rosterVector, originDate)
        {
            CommentId = commentId;
        }
    }
}

using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ResolveCommentAnswerCommand : QuestionCommand
    {
        public Guid CommentId { get; }

        public ResolveCommentAnswerCommand(Guid interviewId, 
            Guid userId, 
            Guid questionId,
            RosterVector rosterVector,
            Guid commentId)
            : base(interviewId, userId, questionId, rosterVector)
        {
            CommentId = commentId;
        }
    }
}

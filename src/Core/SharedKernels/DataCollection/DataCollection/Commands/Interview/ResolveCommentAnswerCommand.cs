using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ResolveCommentAnswerCommand : QuestionCommand
    {
        public ResolveCommentAnswerCommand(Guid interviewId, 
            Guid userId, 
            Guid questionId,
            RosterVector rosterVector)
            : base(interviewId, userId, questionId, rosterVector)
        {
        }
    }
}

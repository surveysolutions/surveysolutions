using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RemoveAnswerCommand : QuestionCommand
    {
        public RemoveAnswerCommand(Guid interviewId,
            Guid userId,
            Identity questionId) 
            : base(interviewId, userId, questionId.Id, questionId.RosterVector)
        {
        }
    }
}

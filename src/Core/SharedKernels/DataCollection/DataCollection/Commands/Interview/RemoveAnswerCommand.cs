using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RemoveAnswerCommand : QuestionCommand
    {
        public DateTime RemoveTime { get; private set; }

        public RemoveAnswerCommand(Guid interviewId,
            Guid userId,
            Identity questionId,
            DateTime removeTime) : base(interviewId, userId, questionId.Id, questionId.RosterVector)
        {
            this.RemoveTime = removeTime;
        }
    }
}
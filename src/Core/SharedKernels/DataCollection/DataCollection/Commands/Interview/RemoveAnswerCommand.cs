using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RemoveAnswerCommand : QuestionCommand
    {
        public DateTime RemoveTime { get; private set; }
        public RemoveAnswerCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector,
            DateTime removeTime) : base(interviewId, userId, questionId, rosterVector)
        {
            this.RemoveTime = removeTime;
        }
    }
}
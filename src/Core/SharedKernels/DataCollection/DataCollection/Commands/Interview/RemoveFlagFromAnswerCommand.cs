using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RemoveFlagFromAnswerCommand : QuestionCommand
    {
        public RemoveFlagFromAnswerCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector)
            : base(interviewId, userId, questionId, rosterVector) {}
    }
}
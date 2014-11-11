using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SetFlagToAnswerCommand : QuestionCommand
    {
        public SetFlagToAnswerCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector)
            : base(interviewId, userId, questionId, rosterVector) {}
    }
}
using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RemoveAnswerCommand : AnswerQuestionCommand
    {
        public RemoveAnswerCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector,
            DateTime answerTime) : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
        }
    }
}
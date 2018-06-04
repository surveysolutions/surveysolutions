using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview.Base
{
    public abstract class AnswerQuestionCommand : QuestionCommand
    {
        protected AnswerQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector)
            : base(interviewId, userId, questionId, rosterVector)
        {
        }
    }
}

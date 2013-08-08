using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview.Base
{
    public abstract class QuestionCommand : InterviewCommand
    {
        public Guid QuestionId { get; private set; }

        protected QuestionCommand(Guid interviewId, Guid userId, Guid questionId)
            : base(interviewId, userId)
        {
            this.QuestionId = questionId;
        }
    }
}
using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview.Base
{
    public abstract class QuestionCommand : InterviewCommand
    {
        public Guid QuestionId { get; private set; }
        public decimal[] RosterVector { get; private set; }

        public Identity Question => new Identity(this.QuestionId, this.RosterVector);

        protected QuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector)
            : base(interviewId, userId)
        {
            this.QuestionId = questionId;
            this.RosterVector = rosterVector;
        }
    }
}

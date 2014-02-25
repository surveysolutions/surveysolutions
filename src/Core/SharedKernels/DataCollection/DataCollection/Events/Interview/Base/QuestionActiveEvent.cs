using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionActiveEvent : InterviewActiveEvent
    {
        public Guid QuestionId { get; private set; }
        public decimal[] PropagationVector { get; private set; }

        protected QuestionActiveEvent(Guid userId, Guid questionId, decimal[] propagationVector)
            : base(userId)
        {
            this.QuestionId = questionId;
            this.PropagationVector = propagationVector;
        }
    }
}
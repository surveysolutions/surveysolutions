using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionPassiveEvent : InterviewPassiveEvent
    {
        public Guid QuestionId { get; private set; }
        public decimal[] PropagationVector { get; private set; }

        protected QuestionPassiveEvent(Guid questionId, decimal[] propagationVector)
        {
            this.QuestionId = questionId;
            this.PropagationVector = propagationVector;
        }
    }
}
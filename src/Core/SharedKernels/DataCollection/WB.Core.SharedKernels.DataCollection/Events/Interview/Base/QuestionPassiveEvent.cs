using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionPassiveEvent : InterviewPassiveEvent
    {
        public Guid QuestionId { get; private set; }
        public int[] PropagationVector { get; private set; }

        protected QuestionPassiveEvent(Guid questionId, int[] propagationVector)
        {
            this.QuestionId = questionId;
            this.PropagationVector = propagationVector;
        }
    }
}
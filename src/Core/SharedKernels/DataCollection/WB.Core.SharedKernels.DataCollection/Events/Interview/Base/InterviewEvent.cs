using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class InterviewEvent
    {
        public Guid UserId { get; private set; }

        protected InterviewEvent(Guid userId)
        {
            this.UserId = userId;
        }
    }
}
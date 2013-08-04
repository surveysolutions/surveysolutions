using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class InterviewActiveEvent
    {
        public Guid UserId { get; private set; }

        protected InterviewActiveEvent(Guid userId)
        {
            this.UserId = userId;
        }
    }
}
using System;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class InterviewActiveEvent : IEvent
    {
        public Guid UserId { get; private set; }

        protected InterviewActiveEvent(Guid userId)
        {
            this.UserId = userId;
        }
    }
}
using System;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class InterviewPassiveEvent : BaseTrackableEvent
    {
        protected InterviewPassiveEvent(DateTimeOffset originDate) : base(originDate){}
    }
}

using System;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class InterviewActiveEvent : BaseTrackableEvent
    {
        public Guid UserId { get; set; }
    }
}

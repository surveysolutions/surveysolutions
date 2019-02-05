using System;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class BaseTrackableEvent : IEvent
    {
        public DateTimeOffset? OriginDate { private set; get; }
    }
}

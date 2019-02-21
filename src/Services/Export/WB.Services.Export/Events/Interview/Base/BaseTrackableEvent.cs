using System;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class BaseTrackableEvent : IEvent
    {
        public DateTimeOffset? OriginDate { get; set; }
    }
}

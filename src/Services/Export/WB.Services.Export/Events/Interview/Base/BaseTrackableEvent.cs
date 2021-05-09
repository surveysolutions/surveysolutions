using System;
using WB.Services.Infrastructure.EventSourcing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class BaseTrackableEvent : IEvent
    {
        public DateTimeOffset? OriginDate { get; set; }
    }
}

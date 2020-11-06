using System;
using System.Collections.Generic;
using Ncqrs.Eventing;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class PostCalendarEventsRequest: ICommunicationMessage
    {
        public CommittedEvent[] Events { get; set; }
        public Guid CalendarEventId { get; set; }
    }
}
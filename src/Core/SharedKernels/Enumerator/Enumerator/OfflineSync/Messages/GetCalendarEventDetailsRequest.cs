using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetCalendarEventDetailsRequest : ICommunicationMessage
    {
        public Guid CalendarEventId { get; set; }
    }
}
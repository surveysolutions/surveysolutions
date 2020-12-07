using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetCalendarEventsRequest: ICommunicationMessage
    {
        public Guid UserId { get; set; }
    }
}
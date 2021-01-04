using System.Collections.Generic;
using Ncqrs.Eventing;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetCalendarEventDetailsResponse: ICommunicationMessage
    {
        public List<CommittedEvent> Events { get; set; }
    }
}
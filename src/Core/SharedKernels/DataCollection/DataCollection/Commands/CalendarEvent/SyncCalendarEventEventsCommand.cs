using System;
using Main.Core.Events;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class SyncCalendarEventEventsCommand : CalendarEventCommand
    {
        public AggregateRootEvent[] SynchronizedEvents { get; set; }
        
        public SyncCalendarEventEventsCommand(AggregateRootEvent[] synchronizedEvents ,Guid publicKey, Guid userId) 
            : base(publicKey, userId)
        {
            this.SynchronizedEvents = synchronizedEvents;
        }
    }
}

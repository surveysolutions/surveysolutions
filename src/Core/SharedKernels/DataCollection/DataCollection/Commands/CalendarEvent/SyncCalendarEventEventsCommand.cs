﻿using System;
using Main.Core.Events;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class SyncCalendarEventEventsCommand : CalendarEventCommand
    {
        public AggregateRootEvent[] SynchronizedEvents { get; set; }
        
        public bool RestoreCalendarEventBefore { get; set; }
        public bool RestoreCalendarEventAfter { get; set; }
        public bool DeleteCalendarEventAfter { get; set; }
        
        public bool ShouldRestorePreviousStateAfterApplying { get; set; }
        
        public SyncCalendarEventEventsCommand(AggregateRootEvent[] synchronizedEvents ,Guid publicKey, Guid userId) 
            : base(publicKey, userId)
        {
            this.SynchronizedEvents = synchronizedEvents;
        }

        public SyncCalendarEventEventsCommand(AggregateRootEvent[] synchronizedEvents, Guid publicKey, Guid userId,
            bool restoreCalendarEventBefore, bool restoreCalendarEventAfter, bool deleteCalendarEventAfter,
            bool shouldRestorePreviousStateAfterApplying) 
            : base(publicKey, userId)
        {
            SynchronizedEvents = synchronizedEvents;
            RestoreCalendarEventBefore = restoreCalendarEventBefore;
            RestoreCalendarEventAfter = restoreCalendarEventAfter;
            DeleteCalendarEventAfter = deleteCalendarEventAfter;
            ShouldRestorePreviousStateAfterApplying = shouldRestorePreviousStateAfterApplying;
        }
    }
}

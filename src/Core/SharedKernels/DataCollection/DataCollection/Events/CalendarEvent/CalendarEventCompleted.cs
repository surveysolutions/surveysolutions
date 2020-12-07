using System;

namespace WB.Core.SharedKernels.DataCollection.Events.CalendarEvent
{
    public class CalendarEventCompleted : CalendarEventEvent
    {
        public CalendarEventCompleted(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
        }
    }
}

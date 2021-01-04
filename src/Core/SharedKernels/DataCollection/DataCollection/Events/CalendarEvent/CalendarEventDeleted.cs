using System;

namespace WB.Core.SharedKernels.DataCollection.Events.CalendarEvent
{
    public class CalendarEventDeleted : CalendarEventEvent
    {
        public CalendarEventDeleted(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
        }
    }
}

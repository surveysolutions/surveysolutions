using System;
using WB.Core.SharedKernels.DataCollection.Events.CalendarEvent;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class CalendarEventRestored : CalendarEventEvent
    {
        public CalendarEventRestored(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
            
        }
    }
}

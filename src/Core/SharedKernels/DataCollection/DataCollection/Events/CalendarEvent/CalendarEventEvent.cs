using System;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.SharedKernels.DataCollection.Events.CalendarEvent
{
    public abstract class CalendarEventEvent : IEvent
    {
        public Guid UserId { get; }
        public DateTimeOffset OriginDate { get; }
        
        protected CalendarEventEvent(Guid userId, DateTimeOffset originDate)
        {
            this.UserId = userId;
            this.OriginDate = originDate;
        }
    }
}

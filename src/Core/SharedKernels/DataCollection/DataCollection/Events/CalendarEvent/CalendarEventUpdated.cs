using System;

namespace WB.Core.SharedKernels.DataCollection.Events.CalendarEvent
{
    public class CalendarEventUpdated : CalendarEventEvent
    {
        public string Comment { get; set; }
        public DateTimeOffset Start { get; set; }
        public CalendarEventUpdated(Guid userId, DateTimeOffset originDate, string comment, DateTimeOffset start) 
            : base(userId, originDate)
        {
            this.Comment = comment;
            this.Start = start;
        }
    }
}

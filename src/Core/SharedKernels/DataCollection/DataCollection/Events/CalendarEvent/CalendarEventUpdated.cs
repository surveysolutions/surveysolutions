using System;

namespace WB.Core.SharedKernels.DataCollection.Events.CalendarEvent
{
    public class CalendarEventUpdated : CalendarEventEvent
    {
        public string Comment { get; set; }
        public DateTime Start { get; set; }
        public CalendarEventUpdated(Guid userId, DateTimeOffset originDate, string comment, DateTime start) 
            : base(userId, originDate)
        {
            this.Comment = comment;
            this.Start = start;
        }
    }
}

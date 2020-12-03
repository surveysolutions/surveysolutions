using System;

namespace WB.Core.SharedKernels.DataCollection.Events.CalendarEvent
{
    public class CalendarEventUpdated : CalendarEventEvent
    {
        public string Comment { get; set; }
        public DateTimeOffset Start { get; set; }
        public string StartTimezone { get; set; }
        public bool IsSystemGenerated { get; set; }

        public CalendarEventUpdated(Guid userId, DateTimeOffset originDate, string comment, DateTimeOffset start,
            string startTimezone, bool isSystemGenerated) 
            : base(userId, originDate)
        {
            this.Comment = comment;
            this.Start = start;
            this.StartTimezone = startTimezone;
            this.IsSystemGenerated = isSystemGenerated;
        }
    }
}

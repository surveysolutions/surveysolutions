using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class UpdateCalendarEventCommand : CalendarEventCommand
    {
        public UpdateCalendarEventCommand(Guid publicKey, 
            Guid userId,
            DateTimeOffset start,
            string startTimezone,
            string comment):base(publicKey, userId)
        {
            this.Start = start;
            this.StartTimezone = startTimezone;
            this.Comment = comment;
        }

        public string Comment { get; set; }
        public DateTimeOffset Start { get; set; }
        public string StartTimezone { get; set; }
    }
}

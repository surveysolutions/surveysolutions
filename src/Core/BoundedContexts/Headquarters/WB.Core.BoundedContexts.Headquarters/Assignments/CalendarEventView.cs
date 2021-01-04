#nullable enable
using System;
using NodaTime;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class CalendarEventView
    {
        public CalendarEventView(ZonedDateTime start, string comment, Guid publicKey)
        {
            StartUtc = start.ToDateTimeUtc();
            StartTimezone = start.Zone.Id;
            Start = start;
            Comment = comment;
            PublicKey = publicKey;
        }

        public DateTime StartUtc { get; set; }
        public string StartTimezone { get; set; }
        public ZonedDateTime Start{ get; set; }
        public string Comment { get; set; }
        public Guid PublicKey { get; set; }
    }
}

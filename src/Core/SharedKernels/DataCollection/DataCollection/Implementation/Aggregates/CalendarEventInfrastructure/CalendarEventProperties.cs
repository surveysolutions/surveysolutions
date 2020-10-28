using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.CalendarEventInfrastructure
{
    public class CalendarEventProperties
    {
        public Guid PublicKey { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset Start { get; set; }
    }
}

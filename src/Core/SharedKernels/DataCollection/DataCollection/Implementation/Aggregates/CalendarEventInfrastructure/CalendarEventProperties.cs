using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.CalendarEventInfrastructure
{
    public class CalendarEventProperties
    {
        public Guid PublicKey { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset Start { get; set; }
        public string StartTimezone { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid? InterviewId { get; set; }
        public string InterviewKey { get; set; }
        public long AssignmentId { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public bool IsDeleted { get; set; }
    }
}

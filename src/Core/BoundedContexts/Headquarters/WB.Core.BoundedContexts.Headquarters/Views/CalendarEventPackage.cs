#nullable enable
using System;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class CalendarEventPackage
    {
        public int Id { get; set; }
        public Guid CalendarEventId { get; set; }
        public Guid ResponsibleId { get; set; }
        public DateTime IncomingDate { get; set; }
        public string Events { get; set; } = String.Empty;
        
        public DateTime LastUpdateDateUtc { get; set; }
        public Guid? InterviewId  { get; set; }
        public int AssignmentId { get; set; }
        public bool IsDeleted { get; set; }
    }
}

using System;

namespace WB.Core.SharedKernels.DataCollection.Events.CalendarEvent
{
    public class CalendarEventCreated : CalendarEventEvent
    {
        public string Comment { get; set; }
        public DateTimeOffset Start { get; set; }
        public CalendarEventCreated(Guid userId, 
            DateTimeOffset originDate, 
            string comment, 
            DateTimeOffset start,
            Guid? interviewId,
            int assignmentId)
            : base(userId, originDate)
        {
            this.Comment = comment;
            this.Start = start;
            this.AssignmentId = assignmentId;
            this.InterviewId = interviewId;
        }

        public Guid? InterviewId { get; set; }

        public int AssignmentId { get; set; }
    }
}

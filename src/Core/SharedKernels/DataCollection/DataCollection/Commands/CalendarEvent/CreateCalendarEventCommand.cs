using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class CreateCalendarEventCommand : CalendarEventCommand
    {
        public CreateCalendarEventCommand(Guid publicKey, 
            Guid userId,
            DateTime start,
            Guid? interviewId,
            int assignmentId,
            string comment):base(publicKey, userId)
        {
            this.Start = start;
            this.Comment = comment;
            this.InterviewId = interviewId;
            this.AssignmentId = assignmentId;
        }

        public string Comment { get; set; }

        public Guid? InterviewId { get; set; }
        public int AssignmentId { get; }
        public DateTime Start { get; set; }
    }
}

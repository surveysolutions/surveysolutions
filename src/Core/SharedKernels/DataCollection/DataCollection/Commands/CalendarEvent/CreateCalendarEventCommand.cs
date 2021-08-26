using System;
using Ncqrs.Commanding;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent
{
    public class CreateCalendarEventCommand : CalendarEventCommand
    {
        public string InterviewKey { set; get; }
        public string Comment { get; set; }
        public Guid? InterviewId { get; set; }
        public int AssignmentId { get; }
        public DateTimeOffset Start { get; set; }
        public string StartTimezone { get; set; }
        
        public CreateCalendarEventCommand(Guid publicKey, 
            Guid userId,
            DateTimeOffset start,
            string startTimezone,
            Guid? interviewId,
            string interviewKey,
            int assignmentId,
            string comment,
            QuestionnaireIdentity questionnaireIdentity):base(publicKey, userId, questionnaireIdentity)
        {
            this.Start = start;
            this.StartTimezone = startTimezone; 
            this.Comment = comment;
            this.InterviewId = interviewId;
            this.InterviewKey = interviewKey;
            this.AssignmentId = assignmentId;
        }
    }
}

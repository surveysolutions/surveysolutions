using System;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CalendarEventChangedArgs
    {
        public CalendarEventChangedArgs(int? assignmentId, Guid interviewId)
        {
            this.AssignmentId = assignmentId;
            this.InterviewId = interviewId;
        }

        public int? AssignmentId { get; }
        public Guid InterviewId { get; }
    }
}
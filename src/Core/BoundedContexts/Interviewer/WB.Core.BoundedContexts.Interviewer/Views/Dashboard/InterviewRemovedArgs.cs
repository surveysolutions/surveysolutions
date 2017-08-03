using System;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class InterviewRemovedArgs
    {
        public InterviewRemovedArgs(int? assignmentId, Guid interviewId)
        {
            this.AssignmentId = assignmentId;
            this.InterviewId = interviewId;
        }

        public int? AssignmentId { get; }
        public Guid InterviewId { get; }
    }
}
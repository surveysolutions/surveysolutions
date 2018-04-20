using System;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities
{
    public class CreateInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public int AssignmentId { get; }
        public string Title { get; }

        public CreateInterviewAuditLogEntity(Guid interviewId, int assignmentId, string title) 
            : base(AuditLogEntityType.CreateInterviewFromAssignment)
        {
            InterviewId = interviewId;
            AssignmentId = assignmentId;
            Title = title;
        }
    }
}

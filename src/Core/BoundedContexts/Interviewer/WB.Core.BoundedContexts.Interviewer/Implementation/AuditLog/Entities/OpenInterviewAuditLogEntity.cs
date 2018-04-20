using System;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities
{
    public class OpenInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; }
        public int? AssignmentId { get; }

        public OpenInterviewAuditLogEntity(Guid interviewId, string interviewKey, int? assignmentId) 
            : base(AuditLogEntityType.OpenInterview)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
            AssignmentId = assignmentId;
        }
    }
}

using System;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities
{
    public class CompleteInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }

        public CompleteInterviewAuditLogEntity(Guid interviewId) : base(AuditLogEntityType.CompleteInterview)
        {
            InterviewId = interviewId;
        }
    }
}

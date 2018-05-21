using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class CompleteInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; set; }

        public CompleteInterviewAuditLogEntity(Guid interviewId, string interviewKey) : base(AuditLogEntityType.CompleteInterview)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
        }
    }
}

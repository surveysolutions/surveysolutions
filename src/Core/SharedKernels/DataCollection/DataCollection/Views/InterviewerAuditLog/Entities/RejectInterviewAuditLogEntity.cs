using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class RejectInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; set; }

        public RejectInterviewAuditLogEntity(Guid interviewId, string interviewKey) : base(AuditLogEntityType.RejectInterview)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
        }
    }
}

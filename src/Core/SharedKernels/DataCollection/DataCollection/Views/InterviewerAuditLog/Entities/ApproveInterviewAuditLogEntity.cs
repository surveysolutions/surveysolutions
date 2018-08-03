using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class ApproveInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; set; }

        public ApproveInterviewAuditLogEntity(Guid interviewId, string interviewKey) : base(AuditLogEntityType.ApproveInterview)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
        }
    }
}

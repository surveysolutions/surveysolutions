using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class CloseInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; }

        public CloseInterviewAuditLogEntity(Guid interviewId, string interviewKey) 
            : base(AuditLogEntityType.CloseInterview)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
        }
    }
}

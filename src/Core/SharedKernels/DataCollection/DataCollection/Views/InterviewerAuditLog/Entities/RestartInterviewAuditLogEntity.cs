using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class RestartInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; set; }

        public RestartInterviewAuditLogEntity(Guid interviewId, string interviewKey) : base(AuditLogEntityType.RestartInterview)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
        }
    }
}

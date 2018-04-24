using System;

namespace WB.Core.SharedKernels.DataCollection.Views.AuditLog.Entities
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

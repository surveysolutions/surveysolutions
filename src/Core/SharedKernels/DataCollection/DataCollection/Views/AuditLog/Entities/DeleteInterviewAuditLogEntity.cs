using System;

namespace WB.Core.SharedKernels.DataCollection.Views.AuditLog.Entities
{
    public class DeleteInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; }
        public int? AssignmentId { get; }

        public DeleteInterviewAuditLogEntity(Guid interviewId, string interviewKey, int? assignmentId) : base(AuditLogEntityType.DeleteInterview)
        {
            this.InterviewId = interviewId;
            this.InterviewKey = interviewKey;
            this.AssignmentId = assignmentId;
        }
    }
}

namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog.Entities
{
    public class CloseInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public string InterviewId { get; }
        public string InterviewKey { get; }

        public CloseInterviewAuditLogEntity(string interviewId, string interviewKey) 
            : base(AuditLogEntityType.CloseInterview)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
        }
    }
}

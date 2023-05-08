using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;

public class RejectInterviewToInterviewerAuditLogEntity : BaseAuditLogEntity
{
    public Guid InterviewId { get; }
    public string InterviewKey { get; set; }
    public string InterviewerName { get; set; }
    public Guid InterviewerId { get; set; }

    public RejectInterviewToInterviewerAuditLogEntity(Guid interviewId, string interviewKey, Guid interviewerId, string interviewerName) 
        : base(AuditLogEntityType.RejectInterviewToInterviewer)
    {
        InterviewId = interviewId;
        InterviewKey = interviewKey;
        InterviewerName = interviewerName;
        InterviewerId = interviewerId;
    }
}
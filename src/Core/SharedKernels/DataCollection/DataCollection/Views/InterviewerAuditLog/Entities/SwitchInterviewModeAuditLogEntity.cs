using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class SwitchInterviewToCawiModeAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; set; }

        public SwitchInterviewToCawiModeAuditLogEntity(Guid interviewId, string interviewKey) 
            : base(AuditLogEntityType.SwitchInterviewToCawiMode)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
        }
    }
}

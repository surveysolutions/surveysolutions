using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class SwitchInterviewModeAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; set; }
        public InterviewMode Mode { get; set; }

        public SwitchInterviewModeAuditLogEntity(Guid interviewId, string interviewKey, InterviewMode mode) 
            : base(AuditLogEntityType.SwitchInterviewMode)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
            Mode = mode;
        }
    }
}

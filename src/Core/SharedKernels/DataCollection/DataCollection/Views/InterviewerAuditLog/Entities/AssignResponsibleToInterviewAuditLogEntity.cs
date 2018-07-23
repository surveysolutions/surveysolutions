using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class AssignResponsibleToInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public string InterviewKey { get; set; }
        public Guid ResponsibleId { get; }
        public string ResponsibleName { get; }

        public AssignResponsibleToInterviewAuditLogEntity(Guid interviewId, string interviewKey, Guid responsibleId, string responsibleName) : base(AuditLogEntityType.AssignResponsibleToInterview)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
            ResponsibleId = responsibleId;
            ResponsibleName = responsibleName;
        }
    }
}

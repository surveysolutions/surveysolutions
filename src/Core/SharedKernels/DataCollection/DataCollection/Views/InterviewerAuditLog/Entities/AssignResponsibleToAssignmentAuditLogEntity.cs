using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class AssignResponsibleToAssignmentAuditLogEntity : BaseAuditLogEntity
    {
        public int AssignmentId { get; }
        public Guid ResponsibleId { get; }
        public string ResponsibleName { get; }

        public AssignResponsibleToAssignmentAuditLogEntity(int assignmentId, Guid responsibleId, string responsibleName) : base(AuditLogEntityType.AssignResponsibleToAssignment)
        {
            AssignmentId = assignmentId;
            ResponsibleId = responsibleId;
            ResponsibleName = responsibleName;
        }
    }
}

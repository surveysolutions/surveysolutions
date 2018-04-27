using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class CreateInterviewAuditLogEntity : BaseAuditLogEntity
    {
        public Guid InterviewId { get; }
        public int AssignmentId { get; }
        public string Title { get; }
        public string InterviewKey { get; set; }

        public CreateInterviewAuditLogEntity(Guid interviewId, int assignmentId, string title, string interviewKey) 
            : base(AuditLogEntityType.CreateInterviewFromAssignment)
        {
            InterviewId = interviewId;
            AssignmentId = assignmentId;
            Title = title;
            InterviewKey = interviewKey;
        }
    }
}

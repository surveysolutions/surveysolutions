using System;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto
{
    public enum AssignmentsImportProcessStatus
    {
        NotStarted = 1,
        Verification,
        VerificationCompleted,
        Import,
        ImportCompleted
    }

    public class AssignmentsImportProcess
    {
        public virtual int Id { get; set; }
        public virtual string QuestionnaireId { get; set; }
        public virtual string FileName { get; set; }
        public virtual int TotalCount { get; set; }
        public virtual string Responsible { get; set; }
        public virtual DateTime StartedDate { get; set; }
        public virtual AssignmentsImportProcessStatus Status { get; set; }
    }
}


using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public class QuestionnaireImportResult
    {
        public Guid ProcessId { get; set; }
        public bool ShouldMigrateAssignments { get; set; }
        public string QuestionnaireTitle { get; set; }
        public string ImportError { get; set; }
        public QuestionnaireIdentity Identity { get; set; }
        public int ProgressPercent { get; set; }
        public QuestionnaireImportStatus Status { get; set; } = QuestionnaireImportStatus.NotStarted;
    }

    public enum QuestionnaireImportStatus
    {
        NotStarted,
        Prepare,
        Progress,
        Finished,
        Error,
    }
}

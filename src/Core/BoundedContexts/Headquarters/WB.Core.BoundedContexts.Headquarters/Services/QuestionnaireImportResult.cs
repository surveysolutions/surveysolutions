
using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public class QuestionnaireImportResult
    {
        public string QuestionnaireTitle { get; set; }
        public string ImportError { get; set; }
        public string QuestionnaireId { get; set; }
        public QuestionnaireIdentity Identity { get; set; }
        public int ProgressPercent { get; set; }
        public QuestionnaireImportStatus Status { get; set; }
        public Guid? MigrateAssignmentProcessId { get; set; }
    }

    public enum QuestionnaireImportStatus
    {
        NotStarted,
        Prepare,
        Progress,
        Finished,
        Error,
        MigrateAssignments,
    }
}


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
        public Progress Progress { get; set; }
        public QuestionnaireImportStatus Status { get; set; }
        public Guid? MigrateAssignmentProcessId { get; set; }
    }

    public class Progress
    {
        public int Current { get; set; } = 0;
        public int Total { get; set; } = 100;
        public double Percent => Current * 100 / Total;

    }

    public enum QuestionnaireImportStatus
    {
        NotStarted,
        Prepare,
        Progress,
        Finished,
        Error,
        MigrateAssignments
    }
}

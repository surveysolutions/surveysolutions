
using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public class QuestionnaireImportResult
    {
        public bool IsSuccess => this.ImportError == null;
        public string QuestionnaireTitle { get; set; }
        public string ImportError { get; set; }
        public QuestionnaireIdentity Identity { get; set; }
        public int Percent { get; set; }
        public QuestionnaireImportStatus Status { get; set; }
        public Guid? MigrateAssignmentProcessId { get; set; }
    }

    public enum QuestionnaireImportStatus
    {
        Progress,
        Finished,
        Error,
        MigrateAssignments
    }
}

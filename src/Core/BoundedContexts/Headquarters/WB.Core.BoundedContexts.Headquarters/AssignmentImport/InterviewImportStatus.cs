using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public enum AssignmentImportStage
    {
        FileVerification = 10,
        AssignmentDataVerification = 20,
        AssignmentCreation = 30
    }

    public class AssignmentImportStatus
    {
        public AssignmentImportStatus()
        {
            this.State = new InterviewImportState { Errors = new List<InterviewImportError>() };
            this.VerificationState = new ImportDataVerificationState();
        }

        public AssignmentImportStage Stage { get; set; }
        public string InterviewImportProcessId { get; set; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public string QuestionnaireTitle { get; set; }
        public bool IsInProgress { get; set; } = false;
        public DateTime StartedDateTime { get; set; }
        public int TotalCount { get; set; }
        public int ProcessedCount { get; set; }
        public double TimePerItem { get; set; }
        public double ElapsedTime { get; set; }
        public double EstimatedTime { get; set; }
        public InterviewImportState State { get; set; }
        public ImportDataVerificationState VerificationState { get; set; }
        public AssignmentImportType AssignmentImportType { get; set; }
    }
}
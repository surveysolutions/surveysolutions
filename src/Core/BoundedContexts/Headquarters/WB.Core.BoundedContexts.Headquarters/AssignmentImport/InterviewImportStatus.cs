using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AssignmentImportStatus
    {
        public AssignmentImportStatus()
        {
            this.State = new InterviewImportState { Errors = new List<InterviewImportError>() };
            this.VerificationState = new ImportDataVerificationState();
        }

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
    }

    public class ImportDataVerificationState
    {
        public ImportDataVerificationState()
        {
            this.WasResponsibleProvided = false;
            this.Errors = new List<PanelImportVerificationError>();
        }

        public List<PanelImportVerificationError> Errors { set; get; }

        public bool WasResponsibleProvided { set; get; }

        public int EntitiesCount { get; set; }

        public int EnumeratorsCount { get; set; }

        public int SupervisorsCount { get; set; }

        public string FileName { get; set; }
    }
}

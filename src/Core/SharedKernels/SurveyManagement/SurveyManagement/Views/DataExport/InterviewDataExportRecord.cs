using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewDataExportRecord 
    {
        public InterviewDataExportRecord(Guid interviewId, string recordId, string[] referenceValues, string parentLevelId,
            ExportedQuestion[] questions)
        {
            this.InterviewId = interviewId;
            this.RecordId = recordId;
            this.ParentRecordId = parentLevelId;
            this.Questions = questions;
            this.ReferenceValues = referenceValues;
        }

        public Guid InterviewId { get; private set; }
        public string RecordId { get; private set; }
        public string ParentRecordId { get; private set; }

        public string[] ReferenceValues { get; private set; }

        public ExportedQuestion[] Questions { get; private set; }
    }
}

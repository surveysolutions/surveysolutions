using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class CumulativeReportStatusChange : IReadSideRepositoryEntity
    {
        protected CumulativeReportStatusChange() {}

        public CumulativeReportStatusChange(string entryId, Guid questionnaireId, long questionnaireVersion, DateTime date, InterviewStatus status, int changeValue)
        {
            this.EntryId = entryId;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Date = date;
            this.Status = status;
            this.ChangeValue = changeValue;
        }

        public virtual string EntryId { get; set; }

        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual DateTime Date { get; set; }

        public virtual InterviewStatus Status { get; set; }
        public virtual int ChangeValue { get; set; }
    }
}
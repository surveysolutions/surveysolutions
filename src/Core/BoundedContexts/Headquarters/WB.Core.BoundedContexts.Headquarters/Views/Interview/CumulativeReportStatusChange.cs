using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class CumulativeReportStatusChange : IReadSideRepositoryEntity
    {
        protected CumulativeReportStatusChange() {}

        public CumulativeReportStatusChange(string entryId, Guid questionnaireId, long questionnaireVersion, DateTime date, InterviewStatus status, int changeValue,
            Guid interviewId, long eventSequence)
        {
            this.EntryId = entryId;
            this.QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion).ToString();
            this.Date = date;
            this.Status = status;
            this.ChangeValue = changeValue;
            this.InterviewId = interviewId;
            this.EventSequence = eventSequence;
        }

        public virtual string EntryId { get; set; }

        public virtual string QuestionnaireIdentity { get; set; }

        public virtual Guid InterviewId { get; set; }

        public virtual long EventSequence { get; set; }
      
        public virtual DateTime Date { get; set; }

        public virtual InterviewStatus Status { get; set; }

        public virtual int ChangeValue { get; set; }
    }
}
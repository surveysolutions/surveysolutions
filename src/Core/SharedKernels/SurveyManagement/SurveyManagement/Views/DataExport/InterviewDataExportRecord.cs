using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewDataExportRecord : IReadSideRepositoryEntity
    {
        protected InterviewDataExportRecord()
        {
        }

        public InterviewDataExportRecord(string recordId, 
            string[] referenceValues, 
            string[] parentLevelIds,
            string[] systemVariableValues)
        {
            this.RecordId = recordId;
            this.ParentRecordIds = parentLevelIds;
            this.ReferenceValues = referenceValues;
            this.SystemVariableValues = systemVariableValues;
        }

        public virtual string Id { get; set; }
        public virtual string RecordId { get; set; }
        public virtual Guid InterviewId { get; set; }
        public virtual string LevelName { get; set; }
        public virtual string[] ParentRecordIds { get; set; }
        public virtual string[] ReferenceValues { get; set; }
        public virtual string[] SystemVariableValues { set; get; }
        public virtual string[] Answers { get; set; }

        public virtual IList<ExportedQuestion> GetQuestions() =>
            this.Answers
            .Select(x =>new ExportedQuestion{Answers = x.Split(ExportFileSettings.NotReadableAnswersSeparator)})
            .ToList();
    }
}

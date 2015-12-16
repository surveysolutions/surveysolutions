using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewDataExportRecord : IReadSideRepositoryEntity
    {
        public InterviewDataExportRecord(string recordId, 
            string[] referenceValues, 
            string[] parentLevelIds,
            IEnumerable<ExportedQuestion> questions, 
            string[] systemVariableValues)
        {
            this.RecordId = recordId;
            this.ParentRecordIds = parentLevelIds;
            this.Questions = new List<ExportedQuestion>(questions);
            this.ReferenceValues = referenceValues;
            this.SystemVariableValues = systemVariableValues;
        }

        public Guid Id { get; set; }

        public string RecordId { get; set; }

        public string[] ParentRecordIds { get; set; }

        public Guid InterviewId { get; set; }

        public string LevelName { get; set; }

        public string[] ReferenceValues { get; set; }

        public string[] SystemVariableValues { set; get; }

        public IList<ExportedQuestion> Questions { get; set; }
    }
}

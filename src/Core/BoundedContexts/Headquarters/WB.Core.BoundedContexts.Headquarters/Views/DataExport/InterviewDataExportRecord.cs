using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
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
        public virtual string[][] Answers { get; set; }

        public virtual IEnumerable<string[]> GetPlainAnswers()
        {
            foreach (var answer in Answers)
            {
                yield return answer;
            }
        }
    }
}

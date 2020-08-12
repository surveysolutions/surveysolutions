﻿using System;
using System.Collections.Generic;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class InterviewDataExportRecord 
    {
        /*protected InterviewDataExportRecord()
        {
        }*/

        public InterviewDataExportRecord(string recordId, 
            string[] referenceValues, 
            string[] parentLevelIds,
            string[] systemVariableValues, 
            string[][] answers)
        {
            this.RecordId = recordId;
            this.ParentRecordIds = parentLevelIds;
            this.ReferenceValues = referenceValues;
            this.SystemVariableValues = systemVariableValues;
            this.Answers = answers;
        }

        public virtual string Id { get; set; } = null!;
        public virtual string RecordId { get; set; }
        public virtual Guid InterviewId { get; set; }
        public virtual string LevelName { get; set; } = null!;
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class InterviewDataExportView 
    {
        public InterviewDataExportView(Guid interviewId,InterviewDataExportLevelView[] levels)
        {
            //this.InterviewKey = interviewKey;
            this.InterviewId = interviewId;
            this.Levels = levels;
        }

        public Guid InterviewId { get; set; }

        public InterviewDataExportLevelView[] Levels { get; set; }

        public IList<InterviewDataExportRecord> Records { get; private set; }

        public static InterviewDataExportView CreateFromRecords(Guid interviewId, IList<InterviewDataExportRecord> records)
        {
            InterviewDataExportLevelView[] levels = records
                .GroupBy(record => record.LevelName)
                .Select(grouping => new InterviewDataExportLevelView(null, grouping.Key, grouping.ToArray()))
                .ToArray();

            return new InterviewDataExportView(interviewId, levels);
        }

        public IEnumerable<InterviewDataExportRecord> GetAsRecords()
        {
            int index = 0;
            foreach (var level in this.Levels)
            foreach (var record in level.Records)
            {
                record.InterviewId = this.InterviewId;
                record.LevelName = level.LevelName;
                record.Id = GenerateRecordId(this.InterviewId, index++);

                yield return record;
            }
        }

        private static string GenerateRecordId(Guid interviewId, int index) => $"{interviewId}${index}";
    }
}
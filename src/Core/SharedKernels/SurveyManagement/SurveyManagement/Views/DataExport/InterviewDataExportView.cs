using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewDataExportView : IReadSideRepositoryEntity
    {
        private InterviewDataExportView(InterviewDataExportLevelView[] levels)
        {
            this.Levels = levels;
        }

        public InterviewDataExportView(Guid interviewId, InterviewDataExportLevelView[] levels)
        {
            this.InterviewId = interviewId;
            this.Levels = levels;
        }

        public Guid InterviewId { get; set; }

        public InterviewDataExportLevelView[] Levels { get; set; }

        public IList<InterviewDataExportRecord> Records { get; private set; }

        public static InterviewDataExportView CreateFromRecords(List<InterviewDataExportRecord> records)
        {
            Guid interviewId = records.Select(record => record.InterviewId).Distinct().Single();

            InterviewDataExportLevelView[] levels = records
                .GroupBy(record => record.LevelName)
                .Select(grouping => new InterviewDataExportLevelView(null, grouping.Key, grouping.ToArray()))
                .ToArray();

            return new InterviewDataExportView(interviewId, levels);
        }

        public IEnumerable<InterviewDataExportRecord> GetAsRecords()
        {
            foreach (var level in this.Levels)
                foreach (var record in level.Records)
                {
                    record.InterviewId = this.InterviewId;
                    record.LevelName = level.LevelName;

                    yield return record;
                }
        }
    }
}

using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewDataExportLevelView
    {
        public InterviewDataExportLevelView(Guid levelId, string levelName, InterviewDataExportRecord[] records)
        {
            this.LevelId = levelId;
            this.LevelName = levelName;
            this.Records = records;
        }
        public Guid LevelId { get; private set; }
        public string LevelName { get; private set; }
        public InterviewDataExportRecord[] Records { get; set; }
    }
}

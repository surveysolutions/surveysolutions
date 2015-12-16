using System;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewDataExportView : IReadSideRepositoryEntity
    {
        public InterviewDataExportView(InterviewDataExportLevelView[] levels)
        {
            this.Levels = levels;
        }

        public Guid InterviewId { get; set; }

        public InterviewDataExportLevelView[] Levels { get; set; }

        public InterviewDataExportRecord[] Records { get; private set; }
    }
}

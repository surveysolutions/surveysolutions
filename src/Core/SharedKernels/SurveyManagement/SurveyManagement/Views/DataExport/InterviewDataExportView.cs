using System;
using System.Collections.Generic;
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

        public IList<InterviewDataExportRecord> Records { get; private set; }
    }
}

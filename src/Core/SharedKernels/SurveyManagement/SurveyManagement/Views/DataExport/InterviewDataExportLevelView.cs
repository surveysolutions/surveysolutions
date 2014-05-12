﻿using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewDataExportLevelView
    {
        public InterviewDataExportLevelView(ValueVector<Guid> levelVector, string levelName, InterviewDataExportRecord[] records)
        {
            this.LevelVector = levelVector;
            this.LevelName = levelName;
            this.Records = records;
        }
        public ValueVector<Guid> LevelVector { get; private set; }
        public string LevelName { get; private set; }
        public InterviewDataExportRecord[] Records { get; set; }
    }
}

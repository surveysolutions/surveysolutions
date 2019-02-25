﻿using System;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Services.Processing
{
    public class RunningDataExportProcessView
    {
        public string DataExportProcessId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public int Progress { get; set; }
        public DataExportType Type { get; set; }
        public DataExportFormat Format { get; set; }
        public InterviewStatus? InterviewStatus { get; set; }
        public string QuestionnaireId { get; set; }
        public DataExportStatus ProcessStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsRunning { get; set; }
        public TimeSpan? TimeEstimation { get; set; }
    }
}

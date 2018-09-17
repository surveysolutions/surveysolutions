using System;
using System.Collections.Concurrent;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.Services.Processing
{
    public class RunningDataExportProcessView
    {
        public string DataExportProcessId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string DataExportProcessName { get; set; }
        public int Progress { get; set; }
        public DataExportType Type { get; set; }
        public DataExportFormat Format { get; set; }
        public InterviewStatus? InterviewStatus { get; set; }
        public QuestionnaireId QuestionnaireId { get; set; }
        public DataExportStatus ProcessStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

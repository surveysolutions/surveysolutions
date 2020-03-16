using System;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Services.Processing
{
    public class DataExportProcessView
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string DataExportProcessId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public int Progress { get; set; }
        public DataExportType Type { get; set; }
        public DataExportFormat Format { get; set; }
        public InterviewStatus? InterviewStatus { get; set; }
        public string QuestionnaireId { get; set; }
        public DataExportStatus ProcessStatus { get; set; }
        public DataExportJobStatus JobStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsRunning { get; set; }
        public TimeSpan? TimeEstimation { get; set; }
        public DataExportErrorView Error { get; set; }
        public bool HasFile { get; set; }
        public double FileSize { get; set; }
        public DateTime DataFileLastUpdateDate { get; set; }
        public string DataDestination { get; set; }
    }
}

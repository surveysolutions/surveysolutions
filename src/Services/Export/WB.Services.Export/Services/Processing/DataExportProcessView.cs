using System;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;

namespace WB.Services.Export.Services.Processing
{
    public class DataExportProcessView
    {
        public long Id { get; set; }
        public string DataExportProcessId { get; set; } = String.Empty;
        public DateTime BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Progress { get; set; }
        public DataExportType Type { get; set; }
        public DataExportFormat Format { get; set; }
        public InterviewStatus? InterviewStatus { get; set; }
        public string QuestionnaireId { get; set; } = String.Empty;
        public DataExportStatus ProcessStatus { get; set; }
        public DataExportJobStatus JobStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsRunning { get; set; }
        public TimeSpan? TimeEstimation { get; set; }
        public DataExportErrorView? Error { get; set; }
        public bool HasFile { get; set; }
        public double FileSize { get; set; }
        public DateTime DataFileLastUpdateDate { get; set; }
        public string DataDestination { get; set; } = String.Empty;
        public Guid? TranslationId { get; set; }

        public bool? IncludeMeta { set; get; }
    }
}

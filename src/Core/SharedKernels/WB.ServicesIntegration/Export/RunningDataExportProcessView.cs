using System;

namespace WB.ServicesIntegration.Export
{
    public class RunningDataExportProcessView
    {
        public long ProcessId { get; set; }

        public DateTime BeginDate { get;  set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastUpdateDate { get;  set; }
        public int Progress { get;  set; }
        public DataExportType Type { get;  set; }
        public DataExportFormat Format { get;  set; }
        public InterviewStatus? InterviewStatus { get;  set; }
        public QuestionnaireIdentity? QuestionnaireIdentity { get; set; }
        public DataExportStatus ProcessStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

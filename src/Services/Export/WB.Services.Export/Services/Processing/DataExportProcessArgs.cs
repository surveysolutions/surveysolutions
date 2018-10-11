using System;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public class DataExportProcessStatus
    {
        public DateTime? BeginDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DataExportStatus Status { get; set; } = DataExportStatus.Queued;
        public int ProgressInPercents { get; set; }
        public bool IsRunning { get; set; }
    }

    public class DataExportProcessArgs
    {
        public DataExportProcessStatus Status { get; set; } = new DataExportProcessStatus();
        public TenantInfo Tenant { get; set; }
        public QuestionnaireId Questionnaire { get; set; }
        public string ArchivePassword { get; set; }

        public string NaturalId => $"{Tenant.Id}${InterviewStatusString()}${this.Format}${this.Questionnaire}${this.StorageTypeString}" +
                                            $"${this.FromDate?.ToString(@"YYYYMMDD") ?? "EMPTY FROM DATE"}" +
                                            $"${this.ToDate?.ToString(@"YYYYMMDD") ?? "EMPTY TO DATE"}";

        public string Name => $"{ArchiveName}" + (StorageType.HasValue ? $" {Enum.GetName(typeof(ExternalStorageType), this.StorageType)}" : "");

        public InterviewStatus? InterviewStatus { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ArchiveName { get; set; }
        public string AccessToken { get; set; }
        public ExternalStorageType? StorageType { get; set; }
        public DataExportFormat Format { get; set; }
        public long ProcessId { get; set; }

        private string StorageTypeString => this.StorageType == null ? "NoExternal" : this.StorageType.ToString();
        private string InterviewStatusString() => InterviewStatus?.ToString() ?? "All";
    }
}

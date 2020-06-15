using System;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Models
{
    public class DateExportProcessError
    {
        public DataExportError Type { get; set; }
        public string Message { get; set; } = String.Empty;
    }

    public class DataExportProcessStatus
    {
        public DateTime CreatedDate { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DataExportStatus Status { get; set; } = DataExportStatus.Queued;
        public DataExportJobStatus JobStatus { get; set; }
        public int ProgressInPercents { get; set; }
        public bool IsRunning { get; set; }
        public TimeSpan? TimeEstimation { get; set; }
        public DateExportProcessError? Error { get; set; }
    }
     
    public class DataExportProcessArgs
    {
        public ExportSettings ExportSettings { get; set; } = null!;
        public DataExportProcessStatus Status { get; set; } = new DataExportProcessStatus();
        public string ArchivePassword { get; set; } = String.Empty;

        public string NaturalId => $"{this.StorageTypeString}${ExportSettings?.NaturalId ?? "noSettings"}";

        public string AccessToken { get; set; } = String.Empty;
        public string RefreshToken { get; set; } = String.Empty;
        public ExternalStorageType? StorageType { get; set; }
        public long ProcessId { get; set; }

        private string StorageTypeString => this.StorageType == null ? "NoExternal" : this.StorageType.ToString();
        public bool ShouldDropTenantSchema { get; set; } = false;
    }
}

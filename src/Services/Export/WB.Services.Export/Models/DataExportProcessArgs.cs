﻿using System;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Models
{
    public class DateExportProcessError
    {
        public DataExportError Type { get; set; }
        public string Message { get; set; }
    }

    public class DataExportProcessStatus
    {
        public DateTime? BeginDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DataExportStatus Status { get; set; } = DataExportStatus.Queued;
        public int ProgressInPercents { get; set; }
        public bool IsRunning { get; set; }
        public TimeSpan? TimeEstimation { get; set; }
        public DateExportProcessError Error { get; set; }
    }

    public class DataExportProcessArgs
    {
        public ExportSettings ExportSettings { get; set; }
        public DataExportProcessStatus Status { get; set; } = new DataExportProcessStatus();
        public string ArchivePassword { get; set; }

        public string NaturalId => $"{this.StorageTypeString}${ExportSettings?.NaturalId ?? "noSettings"}";

        public string AccessToken { get; set; }
        public ExternalStorageType? StorageType { get; set; }
        public long ProcessId { get; set; }

        private string StorageTypeString => this.StorageType == null ? "NoExternal" : this.StorageType.ToString();
    }
}

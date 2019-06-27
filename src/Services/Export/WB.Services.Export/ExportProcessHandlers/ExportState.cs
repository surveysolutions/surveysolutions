﻿using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal class ExportState
    {
        public ExportState(DataExportProcessArgs args)
        {
            ProcessArgs = args;
            Settings = args.ExportSettings;
            Progress = new ExportProgress();
            RequirePublishToExternalStorage = StorageType != null;
        }

        public DataExportProcessArgs ProcessArgs { get; }
        public ExportSettings Settings { get; }
        
        public DataExportFormat ExportFormat => ProcessArgs.ExportSettings.ExportFormat;
        public ExternalStorageType? StorageType => ProcessArgs.StorageType;
        public ExportProgress Progress { get;  }

        public string ExportTempFolder { get; set; }
        public string ArchiveFilePath { get; set; }
        public bool RequireCompression { get; set; } = true;
        public bool RequirePublishToArtifactStorage { get; set; } = true;
        public bool RequirePublishToExternalStorage { get; set; }
        public long ProcessId => ProcessArgs.ProcessId;
    }
}

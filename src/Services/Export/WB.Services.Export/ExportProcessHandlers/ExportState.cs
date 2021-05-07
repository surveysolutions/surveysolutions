using System;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers
{
    public class ExportState
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

        public string ExportTempFolder { get; set; } = String.Empty;
        public string ArchiveFilePath { get; set; } = String.Empty;
        public bool RequireCompression { get; set; } = true;
        public bool RequirePublishToArtifactStorage { get; set; } = true;
        public bool RequirePublishToExternalStorage { get; set; }
        public long ProcessId => ProcessArgs.ProcessId;
        public bool? ShouldDeleteResultExportFile { get; set; }
        public string QuestionnaireName { get; set; } = String.Empty;
    }
}

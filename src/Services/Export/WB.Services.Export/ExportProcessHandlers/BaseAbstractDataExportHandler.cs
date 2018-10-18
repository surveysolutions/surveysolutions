using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers
{
    abstract class BaseAbstractDataExportHandler : IExportProcessHandler<DataExportProcessArgs>
    {
        protected readonly IFileSystemAccessor fileSystemAccessor;
        protected readonly IFileBasedExportedDataAccessor FileBasedExportedDataAccessor;
        protected readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;
        protected readonly IDataExportProcessesService dataExportProcessesService;

        protected string ExportTempDirectoryPath;

        protected BaseAbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportProcessesService = dataExportProcessesService;
            this.FileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.interviewDataExportSettings = interviewDataExportSettings;
        }

        private void HandleProgress(long processId, Progress<int> exportProgress)
        {
            int lastPercent = 0;
            var sw = Stopwatch.StartNew();

            exportProgress.ProgressChanged += (sender, donePercent) =>
            {
                // throttle progress changed events 
                if (donePercent != lastPercent || sw.Elapsed > TimeSpan.FromSeconds(1))
                {
                    this.dataExportProcessesService.UpdateDataExportProgress(processId, donePercent);

                    lastPercent = donePercent;
                    sw.Restart();
                }
            };
        }

        public virtual async Task ExportDataAsync(DataExportProcessArgs dataExportProcessArgs,
            CancellationToken cancellationToken)
        {
            ExportTempDirectoryPath = this.fileSystemAccessor.GetTempPath(interviewDataExportSettings.Value.DirectoryPath);

            cancellationToken.ThrowIfCancellationRequested();

            this.RecreateExportTempDirectory();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var exportProgress = new Progress<int>();

                HandleProgress(dataExportProcessArgs.ProcessId, exportProgress);

                var archiveName = this.FileBasedExportedDataAccessor.GetArchiveFilePathForExportedData(dataExportProcessArgs.ExportSettings);

                this.dataExportProcessesService.ChangeStatusType(
                    dataExportProcessArgs.ProcessId, DataExportStatus.Running);

                await DoExportAsync(dataExportProcessArgs, dataExportProcessArgs.ExportSettings, archiveName, exportProgress, cancellationToken);
            }
            finally
            {
                this.DeleteExportTempDirectory();
            }
        }

        protected abstract Task DoExportAsync(DataExportProcessArgs processArgs,
            ExportSettings exportSettings, string archiveName, IProgress<int> exportProgress, CancellationToken cancellationToken);

        protected abstract DataExportFormat Format { get; }

        private void DeleteExportTempDirectory()
        {
            if (this.fileSystemAccessor.IsDirectoryExists(this.ExportTempDirectoryPath))
                this.fileSystemAccessor.DeleteDirectory(this.ExportTempDirectoryPath);
        }

        private void RecreateExportTempDirectory()
        {
            this.DeleteExportTempDirectory();
            this.fileSystemAccessor.CreateDirectory(this.ExportTempDirectoryPath);
        }
    }
}

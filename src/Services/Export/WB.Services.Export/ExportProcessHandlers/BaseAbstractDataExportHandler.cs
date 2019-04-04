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
    public abstract class BaseAbstractDataExportHandler : IExportProcessHandler<DataExportProcessArgs>
    {
        protected readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IFileBasedExportedDataAccessor fileBasedExportedDataAccessor;
        protected readonly IOptions<ExportServiceSettings> interviewDataExportSettings;
        protected readonly IDataExportProcessesService dataExportProcessesService;

        protected string ExportTempDirectoryPath;

        protected BaseAbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportProcessesService = dataExportProcessesService;
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.interviewDataExportSettings = interviewDataExportSettings;
        }

        private void HandleProgress(DataExportProcessArgs process, ExportProgress exportProgress)
        {
            var sw = Stopwatch.StartNew();

            exportProgress.ProgressChanged += (sender, progress) =>
            {
                if (sw.Elapsed > TimeSpan.FromSeconds(1))
                {
                    lock (sw)
                    {
                        if (sw.Elapsed > TimeSpan.FromSeconds(1))
                        {
                            this.dataExportProcessesService.UpdateDataExportProgress(process.ProcessId,
                                progress.Percent,
                                progress.Eta ?? default);

                            sw.Restart();
                        }
                    }
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

                var exportProgress = new ExportProgress();

                HandleProgress(dataExportProcessArgs, exportProgress);

                var archiveName = this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedData(dataExportProcessArgs.ExportSettings);
                
                this.dataExportProcessesService.ChangeStatusType(dataExportProcessArgs.ProcessId, DataExportStatus.Running);

                await DoExportAsync(dataExportProcessArgs, dataExportProcessArgs.ExportSettings, archiveName, exportProgress, cancellationToken);
            }
            finally
            {
                this.DeleteExportTempDirectory();
            }
        }

        public abstract Task DoExportAsync(DataExportProcessArgs processArgs,
            ExportSettings exportSettings, string archiveName, ExportProgress exportProgress, CancellationToken cancellationToken);

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

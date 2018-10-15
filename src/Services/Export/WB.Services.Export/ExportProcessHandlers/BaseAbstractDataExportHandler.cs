using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers
{
    abstract class BaseAbstractDataExportHandler : IExportProcessHandler<DataExportProcessArgs>
    {
        protected readonly IFileSystemAccessor fileSystemAccessor;
        protected readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        protected readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;
        protected readonly IDataExportProcessesService dataExportProcessesService;

        protected string exportTempDirectoryPath;

        protected BaseAbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportProcessesService = dataExportProcessesService;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
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

        public virtual async Task ExportDataAsync(DataExportProcessArgs dataExportProcessArgs, CancellationToken cancellationToken)
        {
            exportTempDirectoryPath = this.fileSystemAccessor.GetTempPath(interviewDataExportSettings.Value.DirectoryPath);
            
            cancellationToken.ThrowIfCancellationRequested();

            this.RecreateExportTempDirectory();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var exportProgress = new Progress<int>();

                HandleProgress(dataExportProcessArgs.ProcessId, exportProgress);

                var exportSettings = new ExportSettings
                {
                    QuestionnaireId = dataExportProcessArgs.Questionnaire,
                    InterviewStatus = dataExportProcessArgs.InterviewStatus,
                    FromDate = dataExportProcessArgs.FromDate,
                    ToDate = dataExportProcessArgs.ToDate,
                    ExportTempDirectory = this.exportTempDirectoryPath,
                    Tenant = dataExportProcessArgs.Tenant,
                    ArchiveName = dataExportProcessArgs.ArchiveName
                };

                var archiveName = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                    dataExportProcessArgs.Tenant,
                    dataExportProcessArgs.ArchiveName,
                    Format,
                    dataExportProcessArgs.InterviewStatus,
                    fromDate: dataExportProcessArgs.FromDate, toDate: dataExportProcessArgs.ToDate);

                this.dataExportProcessesService.ChangeStatusType(
                    dataExportProcessArgs.ProcessId, DataExportStatus.Running);

                await DoExportAsync(dataExportProcessArgs, exportSettings, archiveName, exportProgress,
                    cancellationToken);
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
            if (this.fileSystemAccessor.IsDirectoryExists(this.exportTempDirectoryPath))
                this.fileSystemAccessor.DeleteDirectory(this.exportTempDirectoryPath);
        }

        private void RecreateExportTempDirectory()
        {
            this.DeleteExportTempDirectory();
            this.fileSystemAccessor.CreateDirectory(this.exportTempDirectoryPath);
        }
    }
}

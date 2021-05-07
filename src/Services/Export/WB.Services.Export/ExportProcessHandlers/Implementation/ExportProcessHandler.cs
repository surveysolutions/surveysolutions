using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.FileSystem;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    class ExportProcessHandler : IExportProcessHandler<DataExportProcessArgs>
    {
        private readonly IExportHandlerFactory exportHandlerFactory;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IOptions<ExportServiceSettings> interviewDataExportSettings;
        private readonly IDataExportFileAccessor dataExportFileAccessor;
        private readonly IPublisherToExternalStorage publisherToExternalStorage;
        private readonly ILogger<ExportProcessHandler> logger;
        private readonly IFileBasedExportedDataAccessor fileBasedExportedDataAccessor;
        private readonly IArchiveUtils archiveUtils;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IExportFileNameService exportFileNameService;

        public ExportProcessHandler(IExportHandlerFactory exportHandlerFactory,
            IDataExportProcessesService dataExportProcessesService,
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IDataExportFileAccessor dataExportFileAccessor,
            IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IArchiveUtils archiveUtils,
            IPublisherToExternalStorage publisherToExternalStorage,
            ILogger<ExportProcessHandler> logger, 
            IExportFileNameService exportFileNameService)
        {
            this.exportHandlerFactory = exportHandlerFactory;
            this.dataExportProcessesService = dataExportProcessesService;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.dataExportFileAccessor = dataExportFileAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.archiveUtils = archiveUtils;
            this.publisherToExternalStorage = publisherToExternalStorage;
            this.logger = logger;
            this.exportFileNameService = exportFileNameService;
        }

        public async Task ExportDataAsync(DataExportProcessArgs process, CancellationToken cancellationToken)
        {
            var state = new ExportState(process);
            state.Settings.JobId = process.ProcessId;
            var handler = exportHandlerFactory.GetHandler(state.ExportFormat, state.StorageType);
            
            HandleProgress(state);

            await PrepareOutputArchive(state, cancellationToken);

            CreateTemporaryFolder(state);

            try
            {
                // ReSharper disable once InconsistentlySynchronizedField
                this.dataExportProcessesService.ChangeStatusType(state.ProcessId, DataExportStatus.Running);

                logger.LogTrace("Start of data export");
                await handler.ExportDataAsync(state, cancellationToken);

                if (state.RequireCompression)
                {
                    await Compress(state, cancellationToken);
                }

                if (state.RequirePublishToArtifactStorage)
                {
                    await PublishToArtifactStorage(state, cancellationToken);
                }

                if (state.RequirePublishToExternalStorage)
                {
                    await PublishToExternalStorage(state, cancellationToken);
                }
            }
            finally
            {
                if (state.ShouldDeleteResultExportFile == true)
                {
                    File.Delete(state.ArchiveFilePath);
                }

                DeleteExportTempDirectory(state);
            }
        }

        private async Task PublishToExternalStorage(ExportState state, CancellationToken cancellationToken)
        {
            await this.publisherToExternalStorage.PublishToExternalStorage(state, cancellationToken);
        }

        private async Task Compress(ExportState state, CancellationToken cancellationToken)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            this.dataExportProcessesService.ChangeStatusType(state.ProcessId, DataExportStatus.Compressing);

            await this.archiveUtils.ZipDirectoryAsync(state.ExportTempFolder, state.ArchiveFilePath,
                state.ProcessArgs.ArchivePassword, new Progress<int>((v) => state.Progress.Report(v)), cancellationToken);
        }

        private async Task PublishToArtifactStorage(ExportState state, CancellationToken cancellationToken)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            var isFilePublished = await this.dataExportFileAccessor.PublishArchiveToArtifactsStorageAsync(state.Settings.Tenant,
                state.ArchiveFilePath, state.Progress, cancellationToken);

            state.ShouldDeleteResultExportFile = isFilePublished;
        }

        private void CreateTemporaryFolder(ExportState state)
        {
            state.ExportTempFolder = this.fileSystemAccessor.GetTempPath(
                    interviewDataExportSettings.Value.DirectoryPath);

            RecreateExportTempDirectory(state);
        }

        private async Task PrepareOutputArchive(ExportState state, CancellationToken cancellationToken)
        {
            state.ArchiveFilePath = await this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedDataAsync(state.Settings);
            state.QuestionnaireName = await this.exportFileNameService.GetQuestionnaireDirectoryName(state.Settings, cancellationToken);
            
            this.fileSystemAccessor.CreateDirectory(Path.GetDirectoryName(state.ArchiveFilePath));
        }

        private void RecreateExportTempDirectory(ExportState state)
        {
            this.DeleteExportTempDirectory(state);
            this.fileSystemAccessor.CreateDirectory(state.ExportTempFolder);
        }

        private void DeleteExportTempDirectory(ExportState state)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(state.ExportTempFolder))
                this.fileSystemAccessor.DeleteDirectory(state.ExportTempFolder);
        }

        private void HandleProgress(ExportState state)
        {
            var sw = Stopwatch.StartNew();

            state.Progress.ProgressChanged += (sender, progress) =>
            {
                if (sw.Elapsed > TimeSpan.FromSeconds(1))
                {
                    lock (sw)
                    {
                        if (sw.Elapsed > TimeSpan.FromSeconds(1))
                        {
                            this.dataExportProcessesService.UpdateDataExportProgress(state.ProcessId,
                                progress.Percent,
                                progress.Eta ?? default);

                            sw.Restart();
                        }
                    }
                }
            };
        }
    }
}

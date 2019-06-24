using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.FileSystem;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    class ExportProcessHandler : IExportProcessHandler<DataExportProcessArgs>
    {
        private readonly IExportHandlerFactory exportHandlerFactory;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IOptions<ExportServiceSettings> interviewDataExportSettings;
        private readonly IDataExportFileAccessor dataExportFileAccessor;
        private readonly IExternalStorageDataClientFactory externalStorageDataClientFactory;
        private readonly IFileBasedExportedDataAccessor fileBasedExportedDataAccessor;
        private readonly IArchiveUtils archiveUtils;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportProcessHandler(IExportHandlerFactory exportHandlerFactory,
            IDataExportProcessesService dataExportProcessesService,
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IDataExportFileAccessor dataExportFileAccessor,
            IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IArchiveUtils archiveUtils,
            IExternalStorageDataClientFactory externalStorageDataClientFactory,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.exportHandlerFactory = exportHandlerFactory;
            this.dataExportProcessesService = dataExportProcessesService;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.dataExportFileAccessor = dataExportFileAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.archiveUtils = archiveUtils;
            this.externalStorageDataClientFactory = externalStorageDataClientFactory;
            this.questionnaireStorage = questionnaireStorage;
        }

        public async Task ExportDataAsync(DataExportProcessArgs process, CancellationToken cancellationToken)
        {
            var state = new ExportState(process);
            var handler = exportHandlerFactory.GetHandler(state.ExportFormat, state.StorageType);

            HandleProgress(state);
            PrepareOutputArchive(state);

            using (TemporaryFolder(state))
            {
                // ReSharper disable once InconsistentlySynchronizedField
                this.dataExportProcessesService.ChangeStatusType(state.ProcessId, DataExportStatus.Running);
                await handler.ExportDataAsync(state, cancellationToken);

                if (state.RequireCompression)
                {
                    await Compress(state, cancellationToken);

                    if (state.RequirePublishToArtifactStorage)
                    {
                        await PublishToArtifactStorage(state, cancellationToken);
                    }

                    if (state.RequirePublishToExternalStorage)
                    {
                        await PublishToExternalStorage(state, cancellationToken);
                    }
                }
            }
        }

        private async Task PublishToExternalStorage(ExportState state, CancellationToken cancellationToken)
        {
            var dataClient = externalStorageDataClientFactory.GetDataClient(state.StorageType);

            using (dataClient.GetClient(state.ProcessArgs.AccessToken))
            {
                var applicationFolder = await dataClient.CreateApplicationFolderAsync();

                var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(state.Settings.QuestionnaireId, cancellationToken);
                var questionnaireFolder = questionnaire.VariableName ?? questionnaire.QuestionnaireId.ToString();

                var folder = await dataClient.CreateFolderAsync(applicationFolder, questionnaireFolder);

                using (var fileStream = File.OpenRead(state.ArchiveFilePath))
                {
                    var filename = Path.GetFileName(state.ArchiveFilePath);
                    await dataClient.UploadFileAsync(folder, filename, fileStream, fileStream.Length, cancellationToken);
                }
            }
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
            await this.dataExportFileAccessor.PublishArchiveToArtifactsStorageAsync(state.Settings.Tenant,
                state.ArchiveFilePath, state.Progress, cancellationToken);
        }

        private IDisposable TemporaryFolder(ExportState state)
        {
            state.ExportTempFolder = this.fileSystemAccessor.GetTempPath(
                    interviewDataExportSettings.Value.DirectoryPath);

            RecreateExportTempDirectory(state);

            return new Disposer(() => DeleteExportTempDirectory(state));
        }

        private void PrepareOutputArchive(ExportState state)
        {
            state.ArchiveFilePath = this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedData(state.Settings);
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

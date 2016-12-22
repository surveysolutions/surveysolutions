using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Security;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    abstract class AbstractDataExportHandler : IExportProcessHandler<DataExportProcessDetails>
    {
        protected readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IZipArchiveProtectionService archiveUtils;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly ILogger logger;
        private readonly IExportSettings exportSettings;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        private IPlainTransactionManager PlainTransactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();

        protected AbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IZipArchiveProtectionService archiveUtils, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService,
            ILogger logger,
            IExportSettings exportSettings,
            IPlainTransactionManagerProvider plainTransactionManagerProvider)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.dataExportProcessesService = dataExportProcessesService;
            this.logger = logger;
            this.exportSettings = exportSettings;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
        }

        public void ExportData(DataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var folderPathOfDataByQuestionnaire = this.GetFolderPathOfDataByQuestionnaire(dataExportProcessDetails.Questionnaire);

            string outputFolderByStatus = dataExportProcessDetails.InterviewStatus?.ToString() ?? "All";
            string folderForDataExport = this.fileSystemAccessor.CombinePath(folderPathOfDataByQuestionnaire, outputFolderByStatus);

            this.ClearFolder(folderForDataExport);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => UpdateDataExportProgress(dataExportProcessDetails.NaturalId, donePercent);

            this.ExportDataIntoDirectory(dataExportProcessDetails.Questionnaire, dataExportProcessDetails.InterviewStatus, folderForDataExport, exportProgress, dataExportProcessDetails.CancellationToken);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var filesToArchive = this.fileSystemAccessor.GetFilesInDirectory(folderForDataExport);

            RecreateExportArchive(filesToArchive, this.GetArchiveNameForData(dataExportProcessDetails.Questionnaire, dataExportProcessDetails.InterviewStatus));
        }

        protected abstract DataExportFormat Format { get; }

        protected abstract void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken);

        private void UpdateDataExportProgress(string dataExportProcessDetailsId, int progressInPercents)
        {
            this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetailsId,
                progressInPercents);
        }

        private string GetArchiveNameForData(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? interviewStatus)
        {
            return this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(questionnaireIdentity, Format, interviewStatus);
        }

        private string GetFolderPathOfDataByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath,
                $"TemporaryExportDataFor{this.Format}");

            if (!fileSystemAccessor.IsDirectoryExists(pathToExportedData))
                fileSystemAccessor.CreateDirectory(pathToExportedData);

            return this.fileSystemAccessor.CombinePath(pathToExportedData,
                $"{questionnaireIdentity.QuestionnaireId}_{questionnaireIdentity.Version}");
        }

        private void RecreateExportArchive(string[] filesToArchive, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.logger.Info($"{archiveFilePath} existed, deleting");
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }

            this.logger.Debug($"Starting creation of {Path.GetFileName(archiveFilePath)} archive");
            Stopwatch watch = Stopwatch.StartNew();
            var password = this.GetPasswordFromSettings();
            this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath, password);
            watch.Stop();
            this.logger.Info($"Done creation {Path.GetFileName(archiveFilePath)} archive. Spent: {watch.Elapsed:g}");
        }

        private string GetPasswordFromSettings()
        {
            return this.PlainTransactionManager.ExecuteInPlainTransaction(() => 
                this.exportSettings.EncryptionEnforced()
                    ? this.exportSettings.GetPassword()
                    : null);
        }

        private void ClearFolder(string folderName)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(folderName))
                this.fileSystemAccessor.DeleteDirectory(folderName);

            this.fileSystemAccessor.CreateDirectory(folderName);
        }
    }
}
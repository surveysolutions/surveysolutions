using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    abstract class AbstractDataExportHandler : IExportProcessHandler<AllDataExportProcessDetails>, IExportProcessHandler<ApprovedDataExportProcessDetails>
    {
        protected readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";

        protected AbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IArchiveUtils archiveUtils, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.dataExportProcessesService = dataExportProcessesService;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
        }

        public void ExportData(AllDataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(dataExportProcessDetails.Questionnaire), allDataFolder);

            this.ClearFolder(folderForDataExport);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => UpdateDataExportProgress(dataExportProcessDetails.NaturalId, donePercent);

            this.ExportAllDataIntoDirectory(dataExportProcessDetails.Questionnaire, folderForDataExport, exportProgress, dataExportProcessDetails.CancellationToken);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var filesToArchive = this.fileSystemAccessor.GetFilesInDirectory(folderForDataExport);

            RecreateExportArchive(filesToArchive, this.GetArchiveNameForAllData(dataExportProcessDetails.Questionnaire));
        }

        public void ExportData(ApprovedDataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(dataExportProcessDetails.Questionnaire), approvedDataFolder);

            this.ClearFolder(folderForDataExport);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => UpdateDataExportProgress(dataExportProcessDetails.NaturalId, donePercent);

            this.ExportApprovedDataIntoDirectory(dataExportProcessDetails.Questionnaire, folderForDataExport, exportProgress, dataExportProcessDetails.CancellationToken);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var filesToArchive = this.fileSystemAccessor.GetFilesInDirectory(folderForDataExport);

            RecreateExportArchive(filesToArchive, GetArchiveNameForApprovedData(dataExportProcessDetails.Questionnaire));
        }

        protected abstract DataExportFormat Format { get; }

        protected abstract void ExportAllDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken);

        protected abstract void ExportApprovedDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken);

        private void UpdateDataExportProgress(string dataExportProcessDetailsId, int progressInPercents)
        {
            this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetailsId,
                progressInPercents);
        }

        private string GetArchiveNameForApprovedData(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedData(
                questionnaireIdentity, Format);
        }

        private string GetArchiveNameForAllData(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                questionnaireIdentity, Format);
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
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }
            this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath);
        }

        private void ClearFolder(string folderName)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(folderName))
                this.fileSystemAccessor.DeleteDirectory(folderName);

            this.fileSystemAccessor.CreateDirectory(folderName);
        }
    }
}
using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
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
        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";

        protected AbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IArchiveUtils archiveUtils, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
        }

        public void ExportData(AllDataExportProcessDetails dataExportProcessDetails)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(dataExportProcessDetails.Questionnaire), allDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => UpdateDataExportProgress(dataExportProcessDetails, donePercent);

            this.ExportAllDataIntoDirectory(dataExportProcessDetails.Questionnaire, folderForDataExport, exportProgress);

            var filesToArchive = this.fileSystemAccessor.GetFilesInDirectory(folderForDataExport);

            RecreateExportArchive(filesToArchive, this.GetArchiveNameForAllData(dataExportProcessDetails.Questionnaire));
        }

        public void ExportData(ApprovedDataExportProcessDetails dataExportProcessDetails)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(dataExportProcessDetails.Questionnaire), approvedDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => UpdateDataExportProgress(dataExportProcessDetails, donePercent);

            ExportApprovedDataIntoDirectory(dataExportProcessDetails.Questionnaire, folderForDataExport, exportProgress);

            var filesToArchive = this.fileSystemAccessor.GetFilesInDirectory(folderForDataExport);

            RecreateExportArchive(filesToArchive, GetArchiveNameForApprovedData(dataExportProcessDetails.Questionnaire));
        }

        protected abstract DataExportFormat Format { get; }

        protected abstract void ExportAllDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, string directoryPath, IProgress<int> progress);

        protected abstract void ExportApprovedDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, string directoryPath, IProgress<int> progress);

        private void UpdateDataExportProgress(IDataExportProcessDetails dataExportProcessDetails, int progressInPercents)
        {
            if (progressInPercents < 0 || progressInPercents > 100)
                throw new ArgumentException();

            if (dataExportProcessDetails == null || dataExportProcessDetails.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcessDetails.LastUpdateDate = DateTime.UtcNow;
            dataExportProcessDetails.ProgressInPercents = progressInPercents;
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
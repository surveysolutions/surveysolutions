using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    abstract class AbstractDataExportHandler : IExportProcessHandler<AllDataExportProcess>, IExportProcessHandler<ApprovedDataExportProcess>
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

        public void ExportData(AllDataExportProcess process)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity), allDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => UpdateDataExportProgress(process, donePercent);

            this.ExportAllDataIntoDirectory(process.QuestionnaireIdentity, folderForDataExport, exportProgress);

            var filesToArchive = this.fileSystemAccessor.GetFilesInDirectory(folderForDataExport);

            RecreateExportArchive(filesToArchive, this.GetArchiveNameForAllData(process.QuestionnaireIdentity));
        }

        public void ExportData(ApprovedDataExportProcess process)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity), approvedDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => UpdateDataExportProgress(process, donePercent);

            ExportApprovedDataIntoDirectory(process.QuestionnaireIdentity, folderForDataExport, exportProgress);

            var filesToArchive = this.fileSystemAccessor.GetFilesInDirectory(folderForDataExport);

            RecreateExportArchive(filesToArchive, GetArchiveNameForApprovedData(process.QuestionnaireIdentity));
        }

        protected abstract DataExportFormat Format { get; }

        protected abstract void ExportAllDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, string directoryPath, IProgress<int> progress);

        protected abstract void ExportApprovedDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, string directoryPath, IProgress<int> progress);

        private void UpdateDataExportProgress(IDataExportProcess dataExportProcess, int progressInPercents)
        {
            if (progressInPercents < 0 || progressInPercents > 100)
                throw new ArgumentException();

            if (dataExportProcess == null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = progressInPercents;
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
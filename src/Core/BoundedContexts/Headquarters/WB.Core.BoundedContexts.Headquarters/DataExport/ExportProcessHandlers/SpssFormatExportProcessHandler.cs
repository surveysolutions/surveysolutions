using Microsoft;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class SpssFormatExportProcessHandler : IExportProcessHandler<AllDataQueuedProcess>, IExportProcessHandler<ApprovedDataQueuedProcess>
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IArchiveUtils archiveUtils;
        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";
        private const string temporaryTabularExportFolder = "TemporaryTabularExport";
        private readonly string pathToExportedData;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public SpssFormatExportProcessHandler(
            IFileSystemAccessor fileSystemAccessor,
            ITabularFormatExportService tabularFormatExportService,
            IArchiveUtils archiveUtils,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
            InterviewDataExportSettings interviewDataExportSettings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabularFormatExportService = tabularFormatExportService;
            this.archiveUtils = archiveUtils;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;

            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, temporaryTabularExportFolder);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public void ExportData(AllDataQueuedProcess process)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity), allDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => process.ProgressInPercents = donePercent;

            this.tabularFormatExportService
                .ExportInterviewsInTabularFormatAsync(process.QuestionnaireIdentity, folderForDataExport, exportProggress);

            var spssFiles = tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaire(process.QuestionnaireIdentity.QuestionnaireId,
                process.QuestionnaireIdentity.Version, fileSystemAccessor.GetFilesInDirectory(folderForDataExport));

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                process.QuestionnaireIdentity,
                DataExportFormat.SPPS);

            RecreateExportArchive(spssFiles, archiveFilePath);
        }

        public void ExportData(ApprovedDataQueuedProcess process)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity), approvedDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => process.ProgressInPercents = donePercent;

            this.tabularFormatExportService
                .ExportApprovedInterviewsInTabularFormatAsync(process.QuestionnaireIdentity, folderForDataExport, exportProggress);

            var spssFiles = tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaire(process.QuestionnaireIdentity.QuestionnaireId,
                process.QuestionnaireIdentity.Version, fileSystemAccessor.GetFilesInDirectory(folderForDataExport));

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedData(
                process.QuestionnaireIdentity,
                DataExportFormat.SPPS);

            RecreateExportArchive(spssFiles, archiveFilePath);
        }

        private string GetFolderPathOfDataByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData,
                $"{questionnaireIdentity.QuestionnaireId}_{questionnaireIdentity.Version}");
        }

        private void RecreateExportArchive(string[] spssFiles, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }
            this.archiveUtils.ZipFiles(spssFiles, archiveFilePath);
        }


        private void ClearFolder(string folderName)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(folderName))
                this.fileSystemAccessor.DeleteDirectory(folderName);

            this.fileSystemAccessor.CreateDirectory(folderName);
        }
    }
}
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
        private const string temporaryTabularDataForSpssExportFolder = "TemporaryTabularDataForSpss";
        private readonly string pathToExportedData;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;
        private readonly IDataExportProcessesService dataExportProcessesService;

        public SpssFormatExportProcessHandler(
            IFileSystemAccessor fileSystemAccessor,
            ITabularFormatExportService tabularFormatExportService,
            IArchiveUtils archiveUtils,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
            InterviewDataExportSettings interviewDataExportSettings, IDataExportProcessesService dataExportProcessesService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabularFormatExportService = tabularFormatExportService;
            this.archiveUtils = archiveUtils;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
            this.dataExportProcessesService = dataExportProcessesService;

            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, temporaryTabularDataForSpssExportFolder);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public void ExportData(AllDataQueuedProcess process)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity), allDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => this.dataExportProcessesService.UpdateDataExportProgress(process.DataExportProcessId, donePercent);

            this.tabularFormatExportService
                .ExportInterviewsInTabularFormatAsync(process.QuestionnaireIdentity, folderForDataExport, exportProggress);

            var spssFiles = tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaire(process.QuestionnaireIdentity.QuestionnaireId,
                process.QuestionnaireIdentity.Version, fileSystemAccessor.GetFilesInDirectory(folderForDataExport));

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                process.QuestionnaireIdentity,
                DataExportFormat.SPSS);

            RecreateExportArchive(spssFiles, archiveFilePath);
        }

        public void ExportData(ApprovedDataQueuedProcess process)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity), approvedDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => this.dataExportProcessesService.UpdateDataExportProgress(process.DataExportProcessId, donePercent);

            this.tabularFormatExportService
                .ExportApprovedInterviewsInTabularFormatAsync(process.QuestionnaireIdentity, folderForDataExport, exportProggress);

            var spssFiles = tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaire(process.QuestionnaireIdentity.QuestionnaireId,
                process.QuestionnaireIdentity.Version, fileSystemAccessor.GetFilesInDirectory(folderForDataExport));

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedData(
                process.QuestionnaireIdentity,
                DataExportFormat.SPSS);

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
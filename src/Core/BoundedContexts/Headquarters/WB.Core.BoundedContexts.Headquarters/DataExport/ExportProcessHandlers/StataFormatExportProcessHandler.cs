using Microsoft;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class StataFormatExportProcessHandler : IExportProcessHandler<AllDataQueuedProcess>, IExportProcessHandler<ApprovedDataQueuedProcess>
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IArchiveUtils archiveUtils;
        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public StataFormatExportProcessHandler(IFileSystemAccessor fileSystemAccessor, ITabularFormatExportService tabularFormatExportService, IArchiveUtils archiveUtils, IFilebasedExportedDataAccessor filebasedExportedDataAccessor, ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabularFormatExportService = tabularFormatExportService;
            this.archiveUtils = archiveUtils;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        public void ExportData(AllDataQueuedProcess process)
        {
            var questionnaireId = process.QuestionnaireIdentity.QuestionnaireId;
            var questionnaireVersion = process.QuestionnaireIdentity.Version;

            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(
                  this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireId,
                      questionnaireVersion), allDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => process.ProgressInPercents = donePercent;

            this.tabularFormatExportService
                .ExportInterviewsInTabularFormatAsync(process.QuestionnaireIdentity, folderForDataExport, exportProggress);

            var statsFiles = tabularDataToExternalStatPackageExportService.CreateAndGetStataDataFilesForQuestionnaire(questionnaireId,
                questionnaireVersion, fileSystemAccessor.GetFilesInDirectory(folderForDataExport));

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                process.QuestionnaireIdentity,
                DataExportFormat.STATA);

            RecreateExportArchive(statsFiles, archiveFilePath);
        }

        public void ExportData(ApprovedDataQueuedProcess process)
        {
            throw new System.NotImplementedException();
        }

        private void RecreateExportArchive(string[] stataFiles, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }
            this.archiveUtils.ZipFiles(stataFiles, archiveFilePath);
        }


        private void ClearFolder(string folderName)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(folderName))
                this.fileSystemAccessor.DeleteDirectory(folderName);

            this.fileSystemAccessor.CreateDirectory(folderName);
        }
    }
}
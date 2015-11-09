using Microsoft;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;
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

        public StataFormatExportProcessHandler(IFileSystemAccessor fileSystemAccessor, ITabularFormatExportService tabularFormatExportService, IArchiveUtils archiveUtils, IFilebasedExportedDataAccessor filebasedExportedDataAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabularFormatExportService = tabularFormatExportService;
            this.archiveUtils = archiveUtils;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
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
            
            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                questionnaireId,
                questionnaireVersion);

            RecreateExportArchive(folderForDataExport, archiveFilePath);
        }

        public void ExportData(ApprovedDataQueuedProcess process)
        {
            throw new System.NotImplementedException();
        }

        private void RecreateExportArchive(string folderForDataExport, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }

            this.archiveUtils.ZipDirectory(folderForDataExport, archiveFilePath);
        }


        private void ClearFolder(string folderName)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(folderName))
                this.fileSystemAccessor.DeleteDirectory(folderName);

            this.fileSystemAccessor.CreateDirectory(folderName);
        }
    }
}
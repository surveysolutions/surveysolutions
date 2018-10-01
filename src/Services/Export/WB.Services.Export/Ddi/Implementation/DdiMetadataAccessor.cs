using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Ddi.Implementation
{
    internal class DdiMetadataAccessor : IDdiMetadataAccessor
    {
        private readonly IDdiMetadataFactory ddiMetadataFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string ExportedDataFolderName = "DdiMetaData";
        private readonly string pathToDdiMetadata;
        private readonly IExportFileNameService exportFileNameService;
        private readonly IDataExportFileAccessor dataExportFileAccessor;

        public DdiMetadataAccessor(
            IDdiMetadataFactory ddiMetadataFactory, 
            IFileSystemAccessor fileSystemAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings, 
            IExportFileNameService exportFileNameService,
            IDataExportFileAccessor dataExportFileAccessor)
        {
            this.ddiMetadataFactory = ddiMetadataFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportFileNameService = exportFileNameService;
            this.dataExportFileAccessor = dataExportFileAccessor;

            var options = interviewDataExportSettings.Value;
            this.pathToDdiMetadata = fileSystemAccessor.CombinePath(options.DirectoryPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToDdiMetadata))
                fileSystemAccessor.CreateDirectory(this.pathToDdiMetadata);
        }

        public async Task<string> GetFilePathToDDIMetadata(TenantInfo tenant, QuestionnaireId questionnaireId,
            string password)
        {
            var archiveFilePath = this.exportFileNameService.GetFileNameForDdiByQuestionnaire(questionnaireId, this.pathToDdiMetadata);

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                return archiveFilePath;

            var filesToArchive = new List<string>
            {
                await this.ddiMetadataFactory.CreateDDIMetadataFileForQuestionnaireInFolder(tenant, questionnaireId, this.pathToDdiMetadata)
            };

            dataExportFileAccessor.RecreateExportArchive(this.pathToDdiMetadata, filesToArchive, archiveFilePath, password);

            return archiveFilePath;
        }

        public void ClearFiles()
        {
            this.fileSystemAccessor.DeleteDirectory(this.pathToDdiMetadata);
            this.fileSystemAccessor.CreateDirectory(this.pathToDdiMetadata);
        }
    }
}

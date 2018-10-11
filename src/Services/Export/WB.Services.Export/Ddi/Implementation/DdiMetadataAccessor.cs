using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Ddi.Implementation
{
    internal class DdiMetadataAccessor : IDdiMetadataAccessor
    {
        private readonly IDdiMetadataFactory ddiMetadataFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string ExportedDataFolderName = "DdiMetaData";
        private readonly IExportFileNameService exportFileNameService;
        private readonly IDataExportFileAccessor dataExportFileAccessor;
        private readonly InterviewDataExportSettings options;

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

            this.options = interviewDataExportSettings.Value;
            
        }

        public async Task<string> GetFilePathToDDIMetadataAsync(TenantInfo tenant, QuestionnaireId questionnaireId,
            string password)
        {
            var pathToDdiMetadata = fileSystemAccessor.CombinePath(options.DirectoryPath, tenant.Id.Id, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(pathToDdiMetadata))
                fileSystemAccessor.CreateDirectory(pathToDdiMetadata);

            var archiveFilePath = this.exportFileNameService.GetFileNameForDdiByQuestionnaire(questionnaireId, pathToDdiMetadata);

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                return archiveFilePath;

            var filesToArchive = new List<string>
            {
                await this.ddiMetadataFactory.CreateDDIMetadataFileForQuestionnaireInFolder(tenant, questionnaireId, pathToDdiMetadata)
            };

            dataExportFileAccessor.RecreateExportArchive(pathToDdiMetadata, filesToArchive, archiveFilePath, password);

            return archiveFilePath;
        }

        public void ClearFiles(TenantInfo tenant)
        {
            var pathToDdiMetadata = fileSystemAccessor.CombinePath(options.DirectoryPath, tenant.Id.Id, ExportedDataFolderName);

            this.fileSystemAccessor.DeleteDirectory(pathToDdiMetadata);
            this.fileSystemAccessor.CreateDirectory(pathToDdiMetadata);
        }
    }
}

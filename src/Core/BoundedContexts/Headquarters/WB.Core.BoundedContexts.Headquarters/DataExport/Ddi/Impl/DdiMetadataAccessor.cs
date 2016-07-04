using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Ddi.Impl
{
    internal class DdiMetadataAccessor : IDdiMetadataAccessor
    {
        private readonly IArchiveUtils archiveUtils;
        private readonly IDdiMetadataFactory ddiMetadataFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string ExportedDataFolderName = "DdiMetaData";
        private readonly string pathToDdiMetadata;
        private readonly IExportFileNameService exportFileNameService;

        public DdiMetadataAccessor(IArchiveUtils archiveUtils, 
            IDdiMetadataFactory ddiMetadataFactory, 
            IFileSystemAccessor fileSystemAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            IExportFileNameService exportFileNameService)
        {
            this.archiveUtils = archiveUtils;
            this.ddiMetadataFactory = ddiMetadataFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportFileNameService = exportFileNameService;

            this.pathToDdiMetadata = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToDdiMetadata))
                fileSystemAccessor.CreateDirectory(this.pathToDdiMetadata);
        }

        public string GetFilePathToDDIMetadata(QuestionnaireIdentity questionnaireId)
        {
            var archiveFilePath = this.exportFileNameService.GetFileNameForDdiByQuestionnaire(questionnaireId, this.pathToDdiMetadata);

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                return archiveFilePath;

            var filesToArchive = new List<string>
            {
                this.ddiMetadataFactory.CreateDDIMetadataFileForQuestionnaireInFolder(questionnaireId, this.pathToDdiMetadata)
            };

            this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath);

            return archiveFilePath;
        }
    }
}
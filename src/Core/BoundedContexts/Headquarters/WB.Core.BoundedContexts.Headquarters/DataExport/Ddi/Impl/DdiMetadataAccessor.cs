using System.Collections.Generic;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Ddi.Impl
{
    internal class DdiMetadataAccessor : IDdiMetadataAccessor
    {
        private readonly IArchiveUtils archiveUtils;
        private readonly IDdiMetadataFactory ddiMetadataFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string ExportedDataFolderName = "DdiMetaData";
        private readonly string pathToDdiMetadata;

        public DdiMetadataAccessor(IArchiveUtils archiveUtils, 
            IDdiMetadataFactory ddiMetadataFactory, 
            IFileSystemAccessor fileSystemAccessor,
            InterviewDataExportSettings interviewDataExportSettings)
        {
            this.archiveUtils = archiveUtils;
            this.ddiMetadataFactory = ddiMetadataFactory;
            this.fileSystemAccessor = fileSystemAccessor;

            this.pathToDdiMetadata = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToDdiMetadata))
                fileSystemAccessor.CreateDirectory(this.pathToDdiMetadata);
        }

        public string GetFilePathToDDIMetadata(QuestionnaireIdentity questionnaireId)
        {
            var archiveFilePath = this.GetArchiveFilePathForDDIMetadata(questionnaireId);

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                return archiveFilePath;

            var filesToArchive = new List<string>
            {
                this.ddiMetadataFactory.CreateDDIMetadataFileForQuestionnaireInFolder(questionnaireId.QuestionnaireId, questionnaireId.Version, this.pathToDdiMetadata)
            };

            this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath);

            return archiveFilePath;
        }

        protected string GetArchiveFilePathForDDIMetadata(QuestionnaireIdentity questionnaireId)
        {
            var archiveName = $"{questionnaireId.QuestionnaireId}_{questionnaireId.Version}_ddi.zip";
            return this.fileSystemAccessor.CombinePath(this.pathToDdiMetadata, archiveName);
        }
    }
}
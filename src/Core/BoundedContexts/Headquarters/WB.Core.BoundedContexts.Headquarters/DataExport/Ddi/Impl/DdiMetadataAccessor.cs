using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
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
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;
        private readonly IPlainTransactionManager transactionManager;

        public DdiMetadataAccessor(IArchiveUtils archiveUtils, 
            IDdiMetadataFactory ddiMetadataFactory, 
            IFileSystemAccessor fileSystemAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires, 
            IPlainTransactionManager transactionManager)
        {
            this.archiveUtils = archiveUtils;
            this.ddiMetadataFactory = ddiMetadataFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaires = questionnaires;
            this.transactionManager = transactionManager;

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
                this.ddiMetadataFactory.CreateDDIMetadataFileForQuestionnaireInFolder(questionnaireId, this.pathToDdiMetadata)
            };

            this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath);

            return archiveFilePath;
        }

        protected string GetArchiveFilePathForDDIMetadata(QuestionnaireIdentity questionnaireId)
        {
            var questionnaireTitle = this.transactionManager.ExecuteInQueryTransaction(() =>
                  questionnaires.GetById(questionnaireId.ToString())?.Title);

            questionnaireTitle = this.fileSystemAccessor.MakeValidFileName(questionnaireTitle) ?? questionnaireId.QuestionnaireId.FormatGuid();

            var archiveName = $"{questionnaireTitle}_{questionnaireId.Version}_ddi.zip";
            return this.fileSystemAccessor.CombinePath(this.pathToDdiMetadata, archiveName);
        }
    }
}
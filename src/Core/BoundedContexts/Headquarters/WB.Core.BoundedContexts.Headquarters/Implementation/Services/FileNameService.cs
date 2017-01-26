using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class ExportExportFileNameService : IExportFileNameService
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;
        private readonly IPlainTransactionManager transactionManager;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportExportFileNameService(
            IPlainTransactionManager transactionManager, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires, 
            IFileSystemAccessor fileSystemAccessor)
        {
            this.transactionManager = transactionManager;
            this.questionnaires = questionnaires;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public string GetFileNameForBatchUploadByQuestionnaire(QuestionnaireIdentity identity)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);
            return $"template_{questionnaireTitle}_v{identity.Version}.zip";
        }

        public string GetFolderNameForParaDataByQuestionnaire(QuestionnaireIdentity identity, string pathToHistoryFiles)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);
            return this.fileSystemAccessor.CombinePath(pathToHistoryFiles, $"{questionnaireTitle}_{identity.Version}");
        }

        public string GetFileNameForDdiByQuestionnaire(QuestionnaireIdentity identity, string pathToDdiMetadata)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);
            return this.fileSystemAccessor.CombinePath(pathToDdiMetadata, $"{questionnaireTitle}_{identity.Version}_ddi.zip");
        }

        public string GetFileNameForTabByQuestionnaire(QuestionnaireIdentity identity, string pathToExportedData, DataExportFormat format, string statusSuffix)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);
            var archiveName = $"{questionnaireTitle}_{identity.Version}_{format}_{statusSuffix}.zip";
            return this.fileSystemAccessor.CombinePath(pathToExportedData, archiveName);
        }

        private string GetQuestionnaireTitle(QuestionnaireIdentity identity)
        {
            var questionnaireTitle = this.transactionManager.ExecuteInPlainTransaction(() => this.questionnaires.GetById(identity.ToString())?.Title);

            questionnaireTitle = this.fileSystemAccessor.MakeValidFileName(questionnaireTitle);
            
            questionnaireTitle = string.IsNullOrWhiteSpace(questionnaireTitle) 
                ? identity.QuestionnaireId.FormatGuid() 
                : questionnaireTitle;

            return questionnaireTitle;
        }
    }
}

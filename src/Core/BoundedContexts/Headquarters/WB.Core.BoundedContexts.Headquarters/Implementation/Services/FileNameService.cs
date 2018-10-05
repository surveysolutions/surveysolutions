using System;
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
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private IPlainTransactionManager transactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();


        public ExportExportFileNameService(
            IPlainTransactionManagerProvider plainTransactionManagerProvider, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires, 
            IFileSystemAccessor fileSystemAccessor)
        {
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.questionnaires = questionnaires;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public string GetFileNameForBatchUploadByQuestionnaire(QuestionnaireIdentity identity)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);
            return $"template_{questionnaireTitle}_v{identity.Version}.zip";
        }

        public string GetFileNameForDdiByQuestionnaire(QuestionnaireIdentity identity)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);
            return $"{questionnaireTitle}_{identity.Version}_ddi.zip";
        }

        public string GetFileNameForTabByQuestionnaire(QuestionnaireIdentity identity, string pathToExportedData,
            DataExportFormat format, string statusSuffix, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);

            var fromDatePrefix = fromDate == null ? "" : $"_{fromDate.Value:yyyyMMddTHHmm}Z";
            var toDatePrefix = toDate == null ? "" : $"_{toDate.Value:yyyyMMddTHHmm}Z";

            var archiveName = $"{questionnaireTitle}_{identity.Version}_{format}_{statusSuffix}{fromDatePrefix}{toDatePrefix}.zip";
            return this.fileSystemAccessor.CombinePath(pathToExportedData, archiveName);
        }

        public string GetFileNameForAssignmentTemplate(QuestionnaireIdentity identity)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);
            return $"{questionnaireTitle}.tab";
        }

        public string GetQuestionnaireTitleWithVersion(QuestionnaireIdentity identity)
        {
            return $"{GetQuestionnaireTitle(identity)}_{identity.Version}";
        }

        private string GetQuestionnaireTitle(QuestionnaireIdentity identity)
        {
            var questionnaire = this.transactionManager.ExecuteInQueryTransaction(() => this.questionnaires.GetById(identity.ToString()));
            var questionnaireTitle = questionnaire.Variable ?? questionnaire.Title;

            questionnaireTitle = this.fileSystemAccessor.MakeValidFileName(questionnaireTitle);
            
            questionnaireTitle = string.IsNullOrWhiteSpace(questionnaireTitle) 
                ? identity.QuestionnaireId.FormatGuid() 
                : questionnaireTitle;

            return questionnaireTitle;
        }
    }
}

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
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportExportFileNameService(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires, 
            IFileSystemAccessor fileSystemAccessor)
        {
            this.questionnaires = questionnaires;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public string GetFileNameForBatchUploadByQuestionnaire(QuestionnaireIdentity identity)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);
            return $"template_{questionnaireTitle}_v{identity.Version}.zip";
        }

        public string GetFileNameForDdiByQuestionnaire(QuestionnaireIdentity identity, string pathToDdiMetadata)
        {
            var questionnaireTitle = GetQuestionnaireTitle(identity);
            return this.fileSystemAccessor.CombinePath(pathToDdiMetadata, $"{questionnaireTitle}_{identity.Version}_ddi.zip");
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

        private string GetQuestionnaireTitle(QuestionnaireIdentity identity)
        {
            var questionnaire = this.questionnaires.GetById(identity.ToString());
            var questionnaireTitle = questionnaire.Variable ?? questionnaire.Title;

            questionnaireTitle = this.fileSystemAccessor.MakeValidFileName(questionnaireTitle);
            
            questionnaireTitle = string.IsNullOrWhiteSpace(questionnaireTitle) 
                ? identity.QuestionnaireId.FormatGuid() 
                : questionnaireTitle;

            return questionnaireTitle;
        }
    }
}

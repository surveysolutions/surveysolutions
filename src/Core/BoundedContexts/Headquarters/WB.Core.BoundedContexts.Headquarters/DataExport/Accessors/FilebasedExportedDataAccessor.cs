using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal class FilebasedExportedDataAccessor : IFilebasedExportedDataAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string ExportedDataFolderName = "ExportedData";
        private readonly string pathToExportedData;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;
        private readonly IPlainTransactionManager transactionManager;

        public FilebasedExportedDataAccessor(
            IFileSystemAccessor fileSystemAccessor,
            InterviewDataExportSettings interviewDataExportSettings,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires,
            IPlainTransactionManager transactionManager)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaires = questionnaires;
            this.transactionManager = transactionManager;
            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public string GetArchiveFilePathForExportedData(QuestionnaireIdentity questionnaireId, DataExportFormat format, InterviewStatus? status = null)
        {
            var questionnaireTitle = this.transactionManager.ExecuteInQueryTransaction(() =>
                   questionnaires.GetById(questionnaireId.ToString())?.Title);

            questionnaireTitle = this.fileSystemAccessor.MakeValidFileName(questionnaireTitle) ?? questionnaireId.QuestionnaireId.FormatGuid();

            var statusSuffix = status != null && format != DataExportFormat.Binary ? status.ToString() : "All";

            var archiveName = $"{questionnaireTitle}_{questionnaireId.Version}_{format}_{statusSuffix}.zip";

            return this.fileSystemAccessor.CombinePath(this.pathToExportedData, archiveName);
        }
    }
}

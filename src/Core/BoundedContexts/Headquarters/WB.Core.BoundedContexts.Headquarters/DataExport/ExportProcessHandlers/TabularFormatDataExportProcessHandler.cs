using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class TabularFormatDataExportProcessHandler : IExportProcessHandler<AllDataQueuedProcess>, IExportProcessHandler<ApprovedDataQueuedProcess>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IEnvironmentContentService environmentContentService;

        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;

        private ITransactionManager TransactionManager
        {
            get { return this.transactionManagerProvider.GetTransactionManager(); }
        }

        public TabularFormatDataExportProcessHandler(
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader,
            ITransactionManagerProvider transactionManagerProvider,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            IFileSystemAccessor fileSystemAccessor, 
            IArchiveUtils archiveUtils, 
            ITabularFormatExportService tabularFormatExportService, 
            IEnvironmentContentService environmentContentService)
        {
            this.questionnaireReader = questionnaireReader;
            this.transactionManagerProvider = transactionManagerProvider;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.tabularFormatExportService = tabularFormatExportService;
            this.environmentContentService = environmentContentService;
        }

        public void ExportData(AllDataQueuedProcess process)
        {
            var folderForDataExport =
              this.fileSystemAccessor.CombinePath(
                  this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(process.QuestionnaireId,
                      process.QuestionnaireVersion), allDataFolder );

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedTabularData(
                process.QuestionnaireId,
                process.QuestionnaireVersion);

            this.CreateExportedDataArchive(folderForDataExport, archiveFilePath, process.QuestionnaireId, process.QuestionnaireVersion,
                p => this.tabularFormatExportService.ExportInterviewsInTabularFormatAsync(process.QuestionnaireId,
                    process.QuestionnaireVersion, p)
                    .WaitAndUnwrapException());
        }

        public void ExportData(ApprovedDataQueuedProcess process)
        {
            var folderForDataExport =
              this.fileSystemAccessor.CombinePath(
                  this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(process.QuestionnaireId,
                      process.QuestionnaireVersion), approvedDataFolder);

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedTabularData(
                process.QuestionnaireId,
                process.QuestionnaireVersion);

            this.CreateExportedDataArchive(folderForDataExport, archiveFilePath, process.QuestionnaireId, process.QuestionnaireVersion,
                p => this.tabularFormatExportService.ExportApprovedInterviewsInTabularFormatAsync(process.QuestionnaireId,
                    process.QuestionnaireVersion, p)
                    .WaitAndUnwrapException());
        }

        private void CreateExportedDataArchive(string folderForDataExport, string archiveFilePath, Guid questionnaireId, long questionnaireVersion, Action<string> exportDataIntoFolder)
        {
            this.ClearFolder(folderForDataExport);

            this.TransactionManager.ExecuteInQueryTransaction(
                () =>
                {
                    exportDataIntoFolder(folderForDataExport);

                    this.environmentContentService.CreateEnvironmentFiles(
                      this.questionnaireReader.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion),
                      folderForDataExport);
                });

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

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
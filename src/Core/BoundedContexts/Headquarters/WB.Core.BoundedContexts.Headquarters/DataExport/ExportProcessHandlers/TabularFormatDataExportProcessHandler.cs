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
            var questionnaireId = process.QuestionnaireIdentity.QuestionnaireId;
            var questionnaireVersion = process.QuestionnaireIdentity.Version;

            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(
                  this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(questionnaireId,
                      questionnaireVersion), allDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => process.ProgressInPercents = donePercent;

            this.tabularFormatExportService
                .ExportInterviewsInTabularFormatAsync(process.QuestionnaireIdentity, folderForDataExport, exportProggress);

            this.environmentContentService.CreateEnvironmentFiles(
                this.questionnaireReader.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion),
                folderForDataExport);

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedTabularData(
                questionnaireId,
                questionnaireVersion);

            RecreateExportArchive(folderForDataExport, archiveFilePath);
        }

        public void ExportData(ApprovedDataQueuedProcess process)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(
                  this.filebasedExportedDataAccessor.GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity.QuestionnaireId,
                      process.QuestionnaireIdentity.Version), approvedDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => process.ProgressInPercents = donePercent;

            this.tabularFormatExportService
                .ExportApprovedInterviewsInTabularFormatAsync(process.QuestionnaireIdentity, folderForDataExport, exportProggress);

            this.environmentContentService.CreateEnvironmentFiles(
                this.questionnaireReader.AsVersioned().Get(process.QuestionnaireIdentity.QuestionnaireId.FormatGuid(), process.QuestionnaireIdentity.Version),
                folderForDataExport);

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedTabularData(
                process.QuestionnaireIdentity.QuestionnaireId,
                process.QuestionnaireIdentity.Version);

            this.RecreateExportArchive(folderForDataExport, archiveFilePath);
        }

        private void RecreateExportArchive(string folderForDataExport, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }

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
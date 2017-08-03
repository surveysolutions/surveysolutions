using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class TabularFormatDataExportHandler : AbstractDataExportHandler
    {
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IEnvironmentContentService environmentContentService;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;

        public TabularFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            ITransactionManagerProvider transactionManagerProvider, 
            ITabularFormatExportService tabularFormatExportService,
            IEnvironmentContentService environmentContentService, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IDataExportProcessesService dataExportProcessesService,
            ILogger logger, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IDataExportFileAccessor dataExportFileAccessor) : 
            base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, dataExportFileAccessor, logger)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.tabularFormatExportService = tabularFormatExportService;
            this.environmentContentService = environmentContentService;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        protected override DataExportFormat Format => DataExportFormat.Tabular;

        protected override void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            this.GenerateDescriptionTxt(questionnaireIdentity, directoryPath);

            this.tabularFormatExportService.ExportInterviewsInTabularFormat(questionnaireIdentity, status, directoryPath, progress, cancellationToken);

            this.CreateDoFilesForQuestionnaire(questionnaireIdentity, directoryPath, cancellationToken);
        }

        private void GenerateDescriptionTxt(QuestionnaireIdentity questionnaireIdentity, string directoryPath)
            => this.tabularFormatExportService.GenerateDescriptionFile(questionnaireIdentity, directoryPath, ExportFileSettings.TabDataFileExtension);

        private void CreateDoFilesForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string directoryPath, CancellationToken cancellationToken)
        {
            this.transactionManagerProvider.GetTransactionManager().ExecuteInQueryTransaction(() =>
            {
                var questionnaireExportStructure =
                    this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(
                        new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

                this.environmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, directoryPath, cancellationToken);
            });
        }
    }
}
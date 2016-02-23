using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class TabularFormatDataExportHandler : AbstractDataExportHandler
    {
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly IQuestionnaireProjectionsRepository questionnaireProjectionsRepository;

        public TabularFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor, 
            IArchiveUtils archiveUtils, 
            InterviewDataExportSettings interviewDataExportSettings, 
            ITransactionManagerProvider transactionManagerProvider, 
            ITabularFormatExportService tabularFormatExportService,
            IEnvironmentContentService environmentContentService, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IDataExportProcessesService dataExportProcessesService,
            ILogger logger, IQuestionnaireProjectionsRepository questionnaireProjectionsRepository) : 
            base(fileSystemAccessor, archiveUtils, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, logger)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.tabularFormatExportService = tabularFormatExportService;
            this.environmentContentService = environmentContentService;
            this.questionnaireProjectionsRepository = questionnaireProjectionsRepository;
        }

        protected override DataExportFormat Format => DataExportFormat.Tabular;

        protected override void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            this.tabularFormatExportService.ExportInterviewsInTabularFormat(questionnaireIdentity, status, directoryPath, progress, cancellationToken);

            this.CreateDoFilesForQuestionnaire(questionnaireIdentity, directoryPath, cancellationToken);
        }

        private void CreateDoFilesForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string directoryPath, CancellationToken cancellationToken)
        {
            this.transactionManagerProvider.GetTransactionManager().ExecuteInQueryTransaction(() =>
            {
                var questionnaireExportStructure =
                    this.questionnaireProjectionsRepository.GetQuestionnaireExportStructure(
                        new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));

                this.environmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, directoryPath, cancellationToken);
            });
        }
    }
}
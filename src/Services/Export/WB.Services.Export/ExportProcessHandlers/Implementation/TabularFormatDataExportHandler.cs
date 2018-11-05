using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.CsvExport;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal class TabularFormatDataExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly IQuestionnaireExportStructureFactory questionnaireExportStructureStorage;

        public TabularFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            ITabularFormatExportService tabularFormatExportService,
            IEnvironmentContentService environmentContentService,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IDataExportProcessesService dataExportProcessesService,
            ILogger<TabularFormatDataExportHandler> logger,
            IQuestionnaireExportStructureFactory questionnaireExportStructureStorage,
            IDataExportFileAccessor dataExportFileAccessor) :
            base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, dataExportFileAccessor)
        {
            this.tabularFormatExportService = tabularFormatExportService;
            this.environmentContentService = environmentContentService;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        protected override DataExportFormat Format => DataExportFormat.Tabular;

        protected override async Task ExportDataIntoDirectoryAsync(ExportSettings settings,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            await this.tabularFormatExportService.GenerateDescriptionFileAsync(settings.Tenant, settings.QuestionnaireId, ExportTempDirectoryPath, ExportFileSettings.TabDataFileExtension);

            await this.tabularFormatExportService.ExportInterviewsInTabularFormatAsync(settings, ExportTempDirectoryPath, progress, cancellationToken);

            await this.CreateDoFilesForQuestionnaireAsync(settings.Tenant, settings.QuestionnaireId, ExportTempDirectoryPath, cancellationToken);
        }

        private async Task CreateDoFilesForQuestionnaireAsync(TenantInfo tenant, QuestionnaireId questionnaireIdentity, string directoryPath, CancellationToken cancellationToken)
        {
            var questionnaireExportStructure = await this.questionnaireExportStructureStorage
                .GetQuestionnaireExportStructureAsync(tenant, questionnaireIdentity);

            this.environmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, directoryPath, cancellationToken);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.CsvExport;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers
{
    internal class TabularFormatDataExportHandler : IExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly IQuestionnaireExportStructureFactory questionnaireExportStructureStorage;

        public TabularFormatDataExportHandler(
            ITabularFormatExportService tabularFormatExportService,
            IEnvironmentContentService environmentContentService,
            IQuestionnaireExportStructureFactory questionnaireExportStructureStorage)
        {
            this.tabularFormatExportService = tabularFormatExportService;
            this.environmentContentService = environmentContentService;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public DataExportFormat Format => DataExportFormat.Tabular;

        public async Task ExportDataAsync(ExportState state, CancellationToken cancellationToken)
        {
            var settings = state.Settings;
            var tempDir = state.ExportTempFolder;

            await this.tabularFormatExportService.GenerateDescriptionFileAsync(settings.Tenant,
                settings.QuestionnaireId, tempDir,
                ExportFileSettings.TabDataFileExtension, cancellationToken);
            await this.tabularFormatExportService.ExportInterviewsInTabularFormatAsync(settings, tempDir,
                state.Progress, cancellationToken);
            await this.CreateDoFilesForQuestionnaireAsync(settings.Tenant, settings.QuestionnaireId, tempDir,
                settings.Translation,
                cancellationToken);
        }

        private async Task CreateDoFilesForQuestionnaireAsync(TenantInfo tenant,
            QuestionnaireIdentity questionnaireIdentity,
            string directoryPath,
            Guid? translation,
            CancellationToken cancellationToken)
        {
            var questionnaireExportStructure = await this.questionnaireExportStructureStorage
                .GetQuestionnaireExportStructureAsync(tenant, questionnaireIdentity, translation);

            this.environmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, directoryPath,
                cancellationToken);
        }
    }
}

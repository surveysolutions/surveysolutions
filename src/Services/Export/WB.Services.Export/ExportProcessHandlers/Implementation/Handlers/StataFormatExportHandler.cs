using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.CsvExport;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers
{
    class StataFormatExportHandler : TabBasedFormatExportHandler, IExportHandler
    {
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public StataFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            ITabularFormatExportService tabularFormatExportService,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService)
            : base(fileSystemAccessor, tabularFormatExportService)
        {
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        public DataExportFormat Format => DataExportFormat.STATA;

        public async Task ExportDataAsync(ExportState state, CancellationToken cancellationToken)
        {
            var settings = state.Settings;
            var tabFiles = await this.CreateTabularDataFilesAsync(state, cancellationToken);

            var exportedFiles = 
                await this.CreateStataDataFilesFromTabularDataFilesAsync(settings.Tenant, settings.QuestionnaireId, state.Settings.Translation, tabFiles, state.Progress, cancellationToken);

            CheckFileListsAndThrow(tabFiles, exportedFiles);

            this.DeleteTabularDataFiles(tabFiles, cancellationToken);

            await this.GenerateDescriptionTxtAsync(settings.Tenant, settings.QuestionnaireId, 
                state.ExportTempFolder, ExportFileSettings.StataDataFileExtension, cancellationToken);
        }

        private async Task<string[]> CreateStataDataFilesFromTabularDataFilesAsync(TenantInfo tenant,
            QuestionnaireId questionnaireIdentity, Guid? translationId, string[] tabDataFiles,
            ExportProgress progress, CancellationToken cancellationToken)
        {
            var exportProgress = new ExportProgress();
            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(50 + (donePercent.Percent / 2), donePercent.Eta);

            return await tabularDataToExternalStatPackageExportService.CreateAndGetStataDataFilesForQuestionnaireAsync(
                 tenant,
                 questionnaireIdentity, translationId,
                 tabDataFiles,
                 exportProgress,
                 cancellationToken);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.CsvExport;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers
{
    class SpssFormatExportHandler : TabBasedFormatExportHandler, IExportHandler
    {
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public SpssFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            ITabularFormatExportService tabularFormatExportService,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService)
            : base(fileSystemAccessor, tabularFormatExportService)
        {
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        public DataExportFormat Format => DataExportFormat.SPSS;

        public async Task ExportDataAsync(ExportState state, CancellationToken cancellationToken)
        {
            var settings = state.Settings;
            var progress = state.Progress;

            var tabFiles = await this.CreateTabularDataFilesAsync(state, cancellationToken);

            var exportedFiles = await this.CreateSpssDataFilesFromTabularDataFilesAsync(state, tabFiles, cancellationToken);

            CheckFileListsAndThrow(tabFiles, exportedFiles);

            this.DeleteTabularDataFiles(tabFiles, cancellationToken);

            await this.GenerateDescriptionTxtAsync(settings.Tenant, settings.QuestionnaireId, 
                state.ExportTempFolder, ExportFileSettings.SpssDataFileExtension, cancellationToken);
        }

        private async Task<string[]> CreateSpssDataFilesFromTabularDataFilesAsync(
            ExportState state, string[] tabDataFiles, CancellationToken cancellationToken)
        {
            var exportProgress = new ExportProgress();
            exportProgress.ProgressChanged +=
                (sender, donePercent) => state.Progress.Report(50 + (donePercent.Percent / 2), donePercent.Eta);

            return await tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaireAsync(
                state.Settings.Tenant, state.Settings.QuestionnaireId, state.Settings.Translation,
                tabDataFiles,
                exportProgress,
                cancellationToken);
        }
    }
}

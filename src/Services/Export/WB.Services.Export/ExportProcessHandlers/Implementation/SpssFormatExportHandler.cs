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
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    public class SpssFormatExportHandler : TabBasedFormatExportHandler
    {
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public SpssFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            ITabularFormatExportService tabularFormatExportService,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
            IDataExportProcessesService dataExportProcessesService,
            ILogger<SpssFormatExportHandler> logger,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings,
                dataExportProcessesService, tabularFormatExportService, dataExportFileAccessor)
        {
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        protected override DataExportFormat Format => DataExportFormat.SPSS;

        protected override async Task ExportDataIntoDirectory(ExportSettings settings, ExportProgress progress,
            CancellationToken cancellationToken)
        {
            var tabFiles = await this.CreateTabularDataFilesAsync(settings, progress, cancellationToken);

            var exportedFiles = await this.CreateSpssDataFilesFromTabularDataFilesAsync(settings.Tenant, settings.QuestionnaireId, tabFiles, progress,
                cancellationToken);

            CheckFileListsAndThrow(tabFiles, exportedFiles);

            this.DeleteTabularDataFiles(tabFiles, cancellationToken);

            await this.GenerateDescriptionTxtAsync(settings.Tenant, settings.QuestionnaireId, ExportTempDirectoryPath,
                ExportFileSettings.SpssDataFileExtension);
        }

        private async Task<string[]> CreateSpssDataFilesFromTabularDataFilesAsync(TenantInfo tenant, QuestionnaireId questionnaireIdentity,
            string[] tabDataFiles,
            ExportProgress progress, CancellationToken cancellationToken)
        {
            var exportProgress = new ExportProgress();
            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(50 + (donePercent.Percent / 2), donePercent.Eta);

            return await tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaireAsync(
                tenant,
                questionnaireIdentity,
                tabDataFiles,
                exportProgress,
                cancellationToken);
        }
    }
}

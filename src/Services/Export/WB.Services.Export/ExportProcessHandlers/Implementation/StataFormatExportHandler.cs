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
    internal class StataFormatExportHandler : TabBasedFormatExportHandler
    {
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public StataFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService,
            ITabularFormatExportService tabularFormatExportService,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
            ILogger<StataFormatExportHandler> logger,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, tabularFormatExportService, dataExportFileAccessor)
        {
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        protected override DataExportFormat Format => DataExportFormat.STATA;

        protected override async Task ExportDataIntoDirectoryAsync(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var tabFiles = await this.CreateTabularDataFilesAsync(settings, progress, cancellationToken);

            await this.CreateStataDataFilesFromTabularDataFilesAsync(settings.Tenant, settings.QuestionnaireId, tabFiles, progress, cancellationToken);

            this.DeleteTabularDataFiles(tabFiles, cancellationToken);

            await this.GenerateDescriptionTxtAsync(settings.Tenant, settings.QuestionnaireId, ExportTempDirectoryPath, ExportFileSettings.StataDataFileExtension);
        }

        private async Task CreateStataDataFilesFromTabularDataFilesAsync(TenantInfo tenant, QuestionnaireId questionnaireIdentity, string[] tabDataFiles,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            var exportProgress = new Progress<int>();
            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(50 + (donePercent / 2));

            await tabularDataToExternalStatPackageExportService.CreateAndGetStataDataFilesForQuestionnaireAsync(
                 tenant,
                 questionnaireIdentity,
                 tabDataFiles,
                 exportProgress,
                 cancellationToken);
        }
    }
}

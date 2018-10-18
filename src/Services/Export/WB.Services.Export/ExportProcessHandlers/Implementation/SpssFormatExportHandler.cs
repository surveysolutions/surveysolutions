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
    internal class SpssFormatExportHandler : TabBasedFormatExportHandler
    {
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public SpssFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
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

        protected override async Task ExportDataIntoDirectoryAsync(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            
            var tabFiles = await this.CreateTabularDataFiles(settings, progress, cancellationToken);

            this.CreateSpssDataFilesFromTabularDataFiles(settings.Tenant, settings.QuestionnaireId, tabFiles, progress,
                cancellationToken);

            this.DeleteTabularDataFiles(tabFiles, cancellationToken);

            this.GenerateDescriptionTxt(settings.Tenant, settings.QuestionnaireId, ExportTempDirectoryPath,
                ExportFileSettings.SpssDataFileExtension);
        }

        private void CreateSpssDataFilesFromTabularDataFiles(TenantInfo tenant, QuestionnaireId questionnaireIdentity,
            string[] tabDataFiles,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            var exportProgress = new Progress<int>();
            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(50 + (donePercent / 2));

            tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaireAsync(
                tenant,
                questionnaireIdentity,
                tabDataFiles,
                exportProgress,
                cancellationToken);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
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
    internal abstract class TabBasedFormatExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;

        protected TabBasedFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor, 
            IOptions<InterviewDataExportSettings> interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService, 
            ITabularFormatExportService tabularFormatExportService,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, dataExportFileAccessor)
        {
            this.tabularFormatExportService = tabularFormatExportService;
        }

        protected void GenerateDescriptionTxt(TenantInfo tenant, QuestionnaireId questionnaireIdentity,
            string directoryPath, string dataFilesExtension)
            => this.tabularFormatExportService.GenerateDescriptionFileAsync(tenant, questionnaireIdentity, directoryPath, dataFilesExtension);

        protected async Task<string[]> CreateTabularDataFiles(ExportSettings exportSettings, IProgress<int> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(donePercent / 2);

            await this.tabularFormatExportService.ExportInterviewsInTabularFormatAsync(
                exportSettings, ExportTempDirectoryPath, exportProgress, cancellationToken);

            return this.fileSystemAccessor.GetFilesInDirectory(ExportTempDirectoryPath);
        }

        protected void DeleteTabularDataFiles(string[] tabDataFiles, CancellationToken cancellationToken)
        {
            foreach (var tabDataFile in tabDataFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                this.fileSystemAccessor.DeleteFile(tabDataFile);
            }
        }
    }
}

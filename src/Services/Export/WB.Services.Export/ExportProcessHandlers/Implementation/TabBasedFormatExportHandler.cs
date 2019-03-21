using System;
using System.Linq;
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
    public abstract class TabBasedFormatExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;

        public TabBasedFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor, 
            IOptions<ExportServiceSettings> interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService, 
            ITabularFormatExportService tabularFormatExportService,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, dataExportFileAccessor)
        {
            this.tabularFormatExportService = tabularFormatExportService;
        }

        protected Task GenerateDescriptionTxtAsync(TenantInfo tenant, QuestionnaireId questionnaireIdentity,
            string directoryPath, string dataFilesExtension)
            => this.tabularFormatExportService.GenerateDescriptionFileAsync(tenant, questionnaireIdentity, directoryPath, dataFilesExtension);

        protected async Task<string[]> CreateTabularDataFilesAsync(ExportSettings exportSettings, ExportProgress progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new ExportProgress();

            exportProgress.ProgressChanged += (sender, state) => progress.Report(new ProgressState
                {
                    Percent = state.Percent / 2,
                    Eta = state.Eta
                });

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

        protected void CheckFileListsAndThrow(string[] listA, string[] listB)
        {
            var listAWithoutExtensions =
                listA.Select(x => this.fileSystemAccessor.GetFileNameWithoutExtension(x)).ToList();

            var listBWithoutExtensions =
                listB.Select(x => this.fileSystemAccessor.GetFileNameWithoutExtension(x)).ToList();

            if (listAWithoutExtensions.Except(listBWithoutExtensions, StringComparer.OrdinalIgnoreCase).Any())
            {
                throw new InvalidOperationException("Export result doesn't match expected result");
            }
        }
    }
}

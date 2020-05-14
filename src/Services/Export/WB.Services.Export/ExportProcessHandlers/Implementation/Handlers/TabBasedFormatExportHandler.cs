using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.CsvExport;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers
{
    abstract class TabBasedFormatExportHandler
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ITabularFormatExportService tabularFormatExportService;

        protected TabBasedFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            ITabularFormatExportService tabularFormatExportService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabularFormatExportService = tabularFormatExportService;
        }

        protected Task GenerateDescriptionTxtAsync(TenantInfo tenant, QuestionnaireId questionnaireIdentity,
            string directoryPath, string dataFilesExtension, CancellationToken cancellationToken)
            => this.tabularFormatExportService.GenerateDescriptionFileAsync(tenant, questionnaireIdentity, directoryPath, dataFilesExtension, cancellationToken);

        protected async Task<string[]> CreateTabularDataFilesAsync(ExportState state, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new ExportProgress();

            exportProgress.ProgressChanged += (sender, progress) => state.Progress.Report(new ProgressState
                {
                    Percent = progress.Percent / 2,
                    Eta = progress.Eta
                });

            await this.tabularFormatExportService.ExportInterviewsInTabularFormatAsync(
                state.Settings, state.ExportTempFolder, exportProgress, cancellationToken);

            return this.fileSystemAccessor.GetFilesInDirectory(state.ExportTempFolder);
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

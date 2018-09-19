using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.CsvExport;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    [Obsolete("KP-11815")]
    internal abstract class TabBasedFormatExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;

        protected TabBasedFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            IOptions<InterviewDataExportSettings> interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService, 
            ITabularFormatExportService tabularFormatExportService,
            ILogger<TabBasedFormatExportHandler> logger,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, dataExportFileAccessor)
        {
            this.tabularFormatExportService = tabularFormatExportService;
        }

        //protected void GenerateDescriptionTxt(QuestionnaireId questionnaireIdentity, string directoryPath, string dataFilesExtension)
        //    => this.tabularFormatExportService.GenerateDescriptionFile(questionnaireIdentity, directoryPath, dataFilesExtension);

        //protected string[] CreateTabularDataFiles(QuestionnaireId questionnaireIdentity, InterviewStatus? status, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken, DateTime? fromDate, DateTime? toDate)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();

        //    var exportProgress = new Progress<int>();

        //    exportProgress.ProgressChanged +=
        //        (sender, donePercent) => progress.Report(donePercent / 2);

        //    this.tabularFormatExportService.ExportInterviewsInTabularFormat(
        //        questionnaireIdentity, status,
        //        directoryPath, exportProgress, cancellationToken, fromDate, toDate);

        //    return this.fileSystemAccessor.GetFilesInDirectory(directoryPath);
        //}

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

using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal abstract class TabBasedFormatExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;

        protected TabBasedFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            InterviewDataExportSettings interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService, 
            ITabularFormatExportService tabularFormatExportService,
            ILogger logger,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, dataExportFileAccessor, logger)
        {
            this.tabularFormatExportService = tabularFormatExportService;
        }

        protected void GenerateDescriptionTxt(QuestionnaireIdentity questionnaireIdentity, string directoryPath, string dataFilesExtension)
            => this.tabularFormatExportService.GenerateDescriptionFile(questionnaireIdentity, directoryPath, dataFilesExtension);

        protected string[] CreateTabularDataFiles(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(donePercent / 2);

            this.tabularFormatExportService.ExportInterviewsInTabularFormat(questionnaireIdentity, status, directoryPath, exportProgress, cancellationToken);

            return this.fileSystemAccessor.GetFilesInDirectory(directoryPath);
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
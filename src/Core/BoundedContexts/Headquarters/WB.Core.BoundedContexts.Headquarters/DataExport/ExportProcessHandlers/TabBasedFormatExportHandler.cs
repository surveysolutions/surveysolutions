using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal abstract class TabBasedFormatExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;

        protected TabBasedFormatExportHandler(IFileSystemAccessor fileSystemAccessor, 
            IArchiveUtils archiveUtils, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            InterviewDataExportSettings interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService, 
            ITabularFormatExportService tabularFormatExportService,
            ILogger logger)
            : base(fileSystemAccessor, archiveUtils, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, logger)
        {
            this.tabularFormatExportService = tabularFormatExportService;
        }

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
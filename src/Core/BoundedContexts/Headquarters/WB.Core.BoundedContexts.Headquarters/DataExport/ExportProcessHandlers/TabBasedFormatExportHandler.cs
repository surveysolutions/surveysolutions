using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal abstract class TabBasedFormatExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;

        protected TabBasedFormatExportHandler(IFileSystemAccessor fileSystemAccessor, IArchiveUtils archiveUtils, IFilebasedExportedDataAccessor filebasedExportedDataAccessor, InterviewDataExportSettings interviewDataExportSettings, IDataExportProcessesService dataExportProcessesService, ITabularFormatExportService tabularFormatExportService)
            : base(fileSystemAccessor, archiveUtils, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService)
        {
            this.tabularFormatExportService = tabularFormatExportService;
        }

        protected string[] CreateAllTabularDataFiles(
            QuestionnaireIdentity questionnaireIdentity, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(donePercent / 2);

            this.tabularFormatExportService.ExportInterviewsInTabularFormat(
                questionnaireIdentity, directoryPath, exportProgress, cancellationToken);

            return this.fileSystemAccessor.GetFilesInDirectory(directoryPath);
        }

        protected string[] CreateApprovedTabularDataFiles(
            QuestionnaireIdentity questionnaireIdentity, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(donePercent / 2);

            this.tabularFormatExportService.ExportApprovedInterviewsInTabularFormat(
                questionnaireIdentity, directoryPath, exportProgress, cancellationToken);

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
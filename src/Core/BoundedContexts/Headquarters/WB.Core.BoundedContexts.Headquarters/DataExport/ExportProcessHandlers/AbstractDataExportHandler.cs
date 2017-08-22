using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    abstract class AbstractDataExportHandler : IExportProcessHandler<DataExportProcessDetails>
    {
        private readonly string exportTempDirectoryPath;

        protected readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IDataExportFileAccessor dataExportFileAccessor;

        protected AbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportProcessesService = dataExportProcessesService;
            this.dataExportFileAccessor = dataExportFileAccessor;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;

            this.exportTempDirectoryPath = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, "ExportTemp");
        }

        public void ExportData(DataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            this.RecreateExportTempDirectory();

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Progress<int>();

            exportProgress.ProgressChanged += (sender, donePercent) =>
                this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, donePercent);

            this.ExportDataIntoDirectory(dataExportProcessDetails.Questionnaire,
                dataExportProcessDetails.InterviewStatus, this.exportTempDirectoryPath, exportProgress,
                dataExportProcessDetails.CancellationToken);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var filesToArchive = this.fileSystemAccessor.GetFilesInDirectory(this.exportTempDirectoryPath);

            var archiveName = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                dataExportProcessDetails.Questionnaire, Format, dataExportProcessDetails.InterviewStatus);

            dataExportFileAccessor.RecreateExportArchive(filesToArchive, archiveName);

            this.DeleteExportTempDirectory();
        }

        protected abstract DataExportFormat Format { get; }

        protected abstract void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status, string directoryPath, IProgress<int> progress,
            CancellationToken cancellationToken);

        private void DeleteExportTempDirectory()
        {
            if (this.fileSystemAccessor.IsDirectoryExists(this.exportTempDirectoryPath))
                this.fileSystemAccessor.DeleteDirectory(this.exportTempDirectoryPath);
        }

        private void RecreateExportTempDirectory()
        {
            this.DeleteExportTempDirectory();
            this.fileSystemAccessor.CreateDirectory(this.exportTempDirectoryPath);
        }
    }
}
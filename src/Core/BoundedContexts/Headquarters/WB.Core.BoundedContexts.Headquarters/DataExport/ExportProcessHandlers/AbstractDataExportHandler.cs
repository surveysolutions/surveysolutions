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
        protected readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        protected readonly InterviewDataExportSettings interviewDataExportSettings;
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
            this.interviewDataExportSettings = interviewDataExportSettings;
        }

        public void ExportData(DataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            if (this.CanDeleteTempFolder)
                this.RecreateExportTempDirectory();

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Progress<int>();

            exportProgress.ProgressChanged += (sender, donePercent) =>
                this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, donePercent);

            this.ExportDataIntoDirectory(dataExportProcessDetails.Questionnaire,
                dataExportProcessDetails.InterviewStatus, this.ExportTempDirectoryPath, exportProgress,
                dataExportProcessDetails.CancellationToken);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var archiveName = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                dataExportProcessDetails.Questionnaire, Format, dataExportProcessDetails.InterviewStatus);
            
            this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, 0);
            this.dataExportProcessesService.ChangeStatusType(dataExportProcessDetails.NaturalId, DataExportStatus.Compressing);

            this.dataExportFileAccessor.RecreateExportArchive(this.ExportTempDirectoryPath, archiveName, exportProgress);

            if (this.CanDeleteTempFolder)
                this.DeleteExportTempDirectory();
        }

        private string ExportTempDirectoryPath => this.fileSystemAccessor.CombinePath(
            interviewDataExportSettings.DirectoryPath, this.ExportDirectoryName);

        protected virtual string ExportDirectoryName => "ExportTemp";
        protected virtual bool CanDeleteTempFolder => true;

        protected abstract DataExportFormat Format { get; }

        protected abstract void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status, string directoryPath, IProgress<int> progress,
            CancellationToken cancellationToken);

        private void DeleteExportTempDirectory()
        {
            if (this.fileSystemAccessor.IsDirectoryExists(this.ExportTempDirectoryPath))
                this.fileSystemAccessor.DeleteDirectory(this.ExportTempDirectoryPath);
        }

        private void RecreateExportTempDirectory()
        {
            this.DeleteExportTempDirectory();
            this.fileSystemAccessor.CreateDirectory(this.ExportTempDirectoryPath);
        }
    }
}
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
        protected class ExportSettings
        {
            public QuestionnaireIdentity QuestionnaireId { get; set; }
            public InterviewStatus? InterviewStatus { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public string ExportDirectory { get; set; }
        }
        protected readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        protected readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IDataExportFileAccessor dataExportFileAccessor;

        private readonly string exportTempDirectoryPath;

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

            exportTempDirectoryPath = this.fileSystemAccessor.CombinePath(
                interviewDataExportSettings.DirectoryPath, "ExportTemp");
        }

        public void ExportData(DataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            this.RecreateExportTempDirectory();

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Progress<int>();

            exportProgress.ProgressChanged += (sender, donePercent) =>
                this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, donePercent);

            var exportSettings = new ExportSettings
            {
                QuestionnaireId = dataExportProcessDetails.Questionnaire,
                InterviewStatus = dataExportProcessDetails.InterviewStatus,
                FromDate = dataExportProcessDetails.FromDate,
                ToDate = dataExportProcessDetails.ToDate,
                ExportDirectory = this.exportTempDirectoryPath
            };

            this.ExportDataIntoDirectory(exportSettings, exportProgress,
                dataExportProcessDetails.CancellationToken);

            if (this.CompressExportedData)
            {
                dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

                var archiveName = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                    dataExportProcessDetails.Questionnaire, Format, dataExportProcessDetails.InterviewStatus,
                    dataExportProcessDetails.FromDate, dataExportProcessDetails.ToDate);

                this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, 0);
                this.dataExportProcessesService.ChangeStatusType(dataExportProcessDetails.NaturalId,
                    DataExportStatus.Compressing);

                this.dataExportFileAccessor.RecreateExportArchive(this.exportTempDirectoryPath, archiveName,
                    exportProgress);
            }

            this.DeleteExportTempDirectory();
        }

        protected virtual bool CompressExportedData { get; } = true;
        
        protected abstract DataExportFormat Format { get; }

        protected abstract void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
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

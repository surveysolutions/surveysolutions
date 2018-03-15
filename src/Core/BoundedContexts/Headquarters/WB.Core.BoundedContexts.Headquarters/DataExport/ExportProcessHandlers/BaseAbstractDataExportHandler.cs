using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    abstract class BaseAbstractDataExportHandler : IExportProcessHandler<DataExportProcessDetails>
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
        protected readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        protected readonly InterviewDataExportSettings interviewDataExportSettings;
        protected readonly IDataExportProcessesService dataExportProcessesService;

        protected readonly string exportTempDirectoryPath;

        protected BaseAbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportProcessesService = dataExportProcessesService;
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
                this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId,
                    donePercent);

            var exportSettings = new ExportSettings
            {
                QuestionnaireId = dataExportProcessDetails.Questionnaire,
                InterviewStatus = dataExportProcessDetails.InterviewStatus,
                FromDate = dataExportProcessDetails.FromDate,
                ToDate = dataExportProcessDetails.ToDate,
                ExportDirectory = this.exportTempDirectoryPath
            };

            var archiveName = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                dataExportProcessDetails.Questionnaire, Format, dataExportProcessDetails.InterviewStatus,
                dataExportProcessDetails.FromDate, dataExportProcessDetails.ToDate);

            DoExport(dataExportProcessDetails, exportSettings, archiveName, exportProgress);

            this.DeleteExportTempDirectory();
        }
        
        protected abstract void DoExport(DataExportProcessDetails dataExportProcessDetails,
            ExportSettings exportSettings, string archiveName, IProgress<int> exportProgress);

        protected abstract DataExportFormat Format { get; }
        
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
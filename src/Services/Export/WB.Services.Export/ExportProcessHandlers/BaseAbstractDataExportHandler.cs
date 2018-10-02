using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.ExportProcessHandlers
{
    public class ExportSettings
    {
        public QuestionnaireId QuestionnaireId { get; set; }
        public InterviewStatus? InterviewStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ExportTempDirectory { get; set; }
        public TenantInfo Tenant { get; set; }
        public string ArchiveName { get; set; }

        public override string ToString()
        {
            return
                $"{Tenant.Id} - {ArchiveName} {QuestionnaireId} {InterviewStatus?.ToString() ?? "AllStatus"} {FromDate}-{ToDate}";
        }
    }

    internal interface ITempPathProvider
    {
        string GetTempPath(string basePath);
    }

    class ManagedThreadAwareTempPathProvider : ITempPathProvider
    {
        public string GetTempPath(string basePath)
        {
            return Path.Combine(basePath, ".temp", "worker_" + Thread.CurrentThread.ManagedThreadId.ToString());
        }
    }

    abstract class BaseAbstractDataExportHandler : IExportProcessHandler<DataExportProcessDetails>
    {
        protected readonly IFileSystemAccessor fileSystemAccessor;
        protected readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        protected readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;
        protected readonly IDataExportProcessesService dataExportProcessesService;

        protected string exportTempDirectoryPath;

        protected BaseAbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportProcessesService = dataExportProcessesService;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.interviewDataExportSettings = interviewDataExportSettings;
        }

        public virtual async Task ExportDataAsync(DataExportProcessDetails dataExportProcessDetails)
        {
            exportTempDirectoryPath = this.fileSystemAccessor.GetTempPath(interviewDataExportSettings.Value.DirectoryPath);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            this.RecreateExportTempDirectory();

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Progress<int>();

            exportProgress.ProgressChanged += (sender, donePercent) =>
                this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.Tenant, dataExportProcessDetails.NaturalId,
                    donePercent);

            var exportSettings = new ExportSettings
            {
                QuestionnaireId = dataExportProcessDetails.Questionnaire,
                InterviewStatus = dataExportProcessDetails.InterviewStatus,
                FromDate = dataExportProcessDetails.FromDate,
                ToDate = dataExportProcessDetails.ToDate,
                ExportTempDirectory = this.exportTempDirectoryPath,
                Tenant = dataExportProcessDetails.Tenant,
                ArchiveName = dataExportProcessDetails.ArchiveName
            };

            var archiveName = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                dataExportProcessDetails.Tenant, 
                dataExportProcessDetails.ArchiveName, 
                Format, 
                dataExportProcessDetails.InterviewStatus,
                fromDate: dataExportProcessDetails.FromDate, toDate: dataExportProcessDetails.ToDate);

            await DoExportAsync(dataExportProcessDetails, exportSettings, archiveName, exportProgress);

            this.DeleteExportTempDirectory();
        }
        
        protected abstract Task DoExportAsync(DataExportProcessDetails processArgs,
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

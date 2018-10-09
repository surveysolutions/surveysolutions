using System;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetArchiveFilePathForExportedData(TenantInfo tenant, string archiveName,
            DataExportFormat format,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);

        string GetExportDirectory(TenantInfo tenant);
        void MoveExportArchive(string tempArchivePath, string archiveName);
    }

    class FilebasedExportedDataAccessor : IFilebasedExportedDataAccessor
    {
        private readonly IExportFileNameService exportFileNameService;
        private readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public FilebasedExportedDataAccessor(
            IExportFileNameService exportFileNameService, 
            IFileSystemAccessor fileSystemAccessor, 
            IOptions<InterviewDataExportSettings> interviewDataExportSettings)
        {
            this.exportFileNameService = exportFileNameService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewDataExportSettings = interviewDataExportSettings;
        }
        
        public string GetArchiveFilePathForExportedData(TenantInfo tenant, string archiveName, DataExportFormat format,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            fromDate = format == DataExportFormat.Binary ? null : fromDate;
            toDate = format == DataExportFormat.Binary ? null : toDate;

            var statusSuffix = status != null && format != DataExportFormat.Binary ? status.ToString() : "All";

            var exportDirectory = GetExportDirectory(tenant);
            CreateIfNeeded(exportDirectory);

            return this.exportFileNameService.GetFileNameForTabByQuestionnaire(
                archiveName, GetExportDirectory(tenant), format, statusSuffix, fromDate, toDate);
        }

        private void CreateIfNeeded(string path)
        {
            if (!fileSystemAccessor.IsDirectoryExists(path))
                fileSystemAccessor.CreateDirectory(path);
        }

        public string GetExportDirectory(TenantInfo tenant)
        {
            return this.fileSystemAccessor.CombinePath(
                interviewDataExportSettings.Value.DirectoryPath,
                tenant.Id.ToString());
        }

        public void MoveExportArchive(string tempArchivePath, string archiveName)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Storage;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Jobs
{
    internal class ExportArchiveHandleService : IExportArchiveHandleService
    {
        private readonly IFilebasedExportedDataAccessor fileBasedExportedDataAccessor;
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IDataExportFileAccessor exportFileAccessor;
        private readonly ILogger<JobsStatusReporting> logger;

        public ExportArchiveHandleService(IFilebasedExportedDataAccessor fileBasedExportedDataAccessor, 
            IExternalFileStorage externalFileStorage, 
            IDataExportFileAccessor exportFileAccessor, 
            ILogger<JobsStatusReporting> logger)
        {
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.externalFileStorage = externalFileStorage;
            this.exportFileAccessor = exportFileAccessor;
            this.logger = logger;
        }

        public async Task ClearAllExportArchives(TenantInfo tenant)
        {
            if (this.externalFileStorage.IsEnabled())
            {
                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(tenant, string.Empty);
                var items = await this.externalFileStorage.ListAsync(externalStoragePath);
                logger.LogInformation("Deleting export archives for tenant: " + tenant + $" - there is {items.Count} files");

                foreach (var file in items)
                {
                    await this.externalFileStorage.RemoveAsync(file.Path);
                }

                return;
            }

            var directory = this.fileBasedExportedDataAccessor.GetExportDirectory(tenant);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        public async Task<DataExportArchive> DownloadArchiveAsync(TenantInfo tenant, string archiveName,
            DataExportFormat dataExportFormat, InterviewStatus? status,
            DateTime? from, DateTime? to)
        {
            string filePath = this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                tenant,
                archiveName,
                dataExportFormat, status, from, to);

            if (this.externalFileStorage.IsEnabled())
            {
                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(tenant, Path.GetFileName(filePath));
                var metadata = await this.externalFileStorage.GetObjectMetadataAsync(externalStoragePath);

                if (metadata != null)
                {
                    return new DataExportArchive
                    {
                        Redirect = new Uri(this.externalFileStorage.GetDirectLink(externalStoragePath, TimeSpan.FromSeconds(10)))
                    };
                }
            }
            else if (File.Exists(filePath))
            {
                return new DataExportArchive
                {
                    FileName = Path.GetFileName(filePath),
                    Data = new FileStream(filePath, FileMode.Open, FileAccess.Read)
                };
            }

            return null;
        }

    }
}
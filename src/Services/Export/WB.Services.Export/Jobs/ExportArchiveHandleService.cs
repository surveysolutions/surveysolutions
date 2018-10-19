using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Storage;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Jobs
{
    internal class ExportArchiveHandleService : IExportArchiveHandleService
    {
        private readonly IFileBasedExportedDataAccessor fileBasedExportedDataAccessor;
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IDataExportFileAccessor exportFileAccessor;
        private readonly ILogger<JobsStatusReporting> logger;
        private readonly IExportFileNameService fileNameService;

        public ExportArchiveHandleService(IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IExternalFileStorage externalFileStorage,
            IDataExportFileAccessor exportFileAccessor,
            ILogger<JobsStatusReporting> logger,
            IExportFileNameService fileNameService)
        {
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.externalFileStorage = externalFileStorage;
            this.exportFileAccessor = exportFileAccessor;
            this.logger = logger;
            this.fileNameService = fileNameService;
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

        public async Task<DataExportArchive> DownloadArchiveAsync(ExportSettings settings, string archiveName)
        {
            if (this.externalFileStorage.IsEnabled())
            {
                var internalFilePath = this.fileNameService.GetFileNameForExportArchive(settings);

                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(
                    settings.Tenant, Path.GetFileName(internalFilePath));

                var metadata = await this.externalFileStorage.GetObjectMetadataAsync(externalStoragePath);

                if (metadata != null)
                {
                    var downloadFileName = this.fileNameService.GetFileNameForExportArchive(settings, archiveName);

                    var uri = this.externalFileStorage.GetDirectLink(externalStoragePath,
                        TimeSpan.FromHours(10), downloadFileName);

                    return new DataExportArchive
                    {
                        Redirect = uri
                    };
                }
            }
            else
            {
                var filePath = this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedData(settings);
                if (File.Exists(filePath))
                {
                    return new DataExportArchive
                    {
                        FileName = archiveName,
                        Data = new FileStream(filePath, FileMode.Open, FileAccess.Read)
                    };
                }
            }

            return null;
        }

    }
}

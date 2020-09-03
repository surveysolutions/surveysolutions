using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Storage;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Jobs
{
    internal class ExportArchiveHandleService : IExportArchiveHandleService
    {
        private readonly IFileBasedExportedDataAccessor fileBasedExportedDataAccessor;
        private readonly IExternalArtifactsStorage externalArtifactsStorage;
        private readonly IDataExportFileAccessor exportFileAccessor;
        private readonly ILogger<JobsStatusReporting> logger;
        private readonly IExportFileNameService fileNameService;

        public ExportArchiveHandleService(IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IExternalArtifactsStorage externalArtifactsStorage,
            IDataExportFileAccessor exportFileAccessor,
            ILogger<JobsStatusReporting> logger,
            IExportFileNameService fileNameService)
        {
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.externalArtifactsStorage = externalArtifactsStorage;
            this.exportFileAccessor = exportFileAccessor;
            this.logger = logger;
            this.fileNameService = fileNameService;
        }

        public async Task ClearAllExportArchives(TenantInfo tenant)
        {
            if (this.externalArtifactsStorage.IsEnabled())
            {
                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(tenant, string.Empty);
                var items = await this.externalArtifactsStorage.ListAsync(externalStoragePath);
                if(items == null) return;

                logger.LogInformation("Deleting export archives for tenant: {tenant} - there is {count} files", tenant, items.Count);

                foreach (var file in items)
                {
                    await this.externalArtifactsStorage.RemoveAsync(file.Path);
                }

                return;
            }

            var directory = this.fileBasedExportedDataAccessor.GetExportDirectory(tenant);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        public async Task<DataExportArchive?> DownloadArchiveAsync(ExportSettings settings, string questionnaireNamePrefixOverride)
        {
            if (this.externalArtifactsStorage.IsEnabled())
            {
                var internalFilePath = await this.fileNameService.GetFileNameForExportArchiveAsync(settings);

                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(
                    settings.Tenant, Path.GetFileName(internalFilePath));

                var metadata = await this.externalArtifactsStorage.GetObjectMetadataAsync(externalStoragePath);

                if (metadata != null)
                {
                    var downloadFileName = await this.fileNameService.GetFileNameForExportArchiveAsync(settings, questionnaireNamePrefixOverride);

                    var uri = this.externalArtifactsStorage.GetDirectLink(externalStoragePath,
                        TimeSpan.FromHours(10), downloadFileName);

                    return new DataExportArchive
                    {
                        Redirect = uri
                    };
                }
            }
            else
            {
                var filePath = await this.fileBasedExportedDataAccessor.GetArchiveFilePathForExportedDataAsync(settings);
                if (File.Exists(filePath))
                {
                    var downloadFileName = await this.fileNameService.GetFileNameForExportArchiveAsync(settings, questionnaireNamePrefixOverride);

                    return new DataExportArchive
                    {
                        FileName = downloadFileName,
                        Data = new FileStream(filePath, FileMode.Open, FileAccess.Read)
                    };
                }
            }

            return null;
        }

    }
}

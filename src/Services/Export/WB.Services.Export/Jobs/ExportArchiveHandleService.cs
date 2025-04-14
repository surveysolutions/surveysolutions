using System;
using System.IO;
using System.Linq;
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

        public async Task RunRetentionPolicy(TenantInfo tenant, int? countToKeep, int? daysToKeep)
        {
            if(!countToKeep.HasValue && !daysToKeep.HasValue)
                return;
            await DeleteExportArchives(tenant, countToKeep, daysToKeep);
        }

        public async Task ClearExportArchives(TenantInfo tenant)
        {
            await DeleteExportArchives(tenant, null, null);
        }
        
        private async Task DeleteExportArchives(TenantInfo tenant, int? countToKeep, int? daysToKeep)
        {
            if (this.externalArtifactsStorage.IsEnabled())
            {
                await DeleteFromExternalStorage(tenant, countToKeep, daysToKeep);
            }
            else
            {
                await DeleteFromLocalStorage(tenant, countToKeep, daysToKeep);
            }
        }

        private async Task DeleteFromExternalStorage(TenantInfo tenant, int? countToKeep, int? daysToKeep)
        {
            var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(tenant, string.Empty);
            var items = await this.externalArtifactsStorage.ListAsync(externalStoragePath);
            if (items == null) return;

            logger.LogInformation(
                "Deleting export archives for tenant: {tenant} - there are total {count} files, daysToKeep: {daysToKeep}, countToKeep: {countToKeep}",
                tenant, items.Count, daysToKeep, countToKeep);

            int? countToDelete = countToKeep.HasValue ? items.Count - countToKeep.Value : null;

            foreach (var file in items.OrderBy(x => x.LastModified))
            {
                await DeleteFile(file.Path, file.LastModified, tenant, daysToKeep, countToDelete, async path =>
                {
                    await this.externalArtifactsStorage.RemoveAsync(path);
                });
                if (countToDelete.HasValue) countToDelete--;
            }
        }

        private async Task DeleteFromLocalStorage(TenantInfo tenant, int? countToKeep, int? daysToKeep)
        {
            var directory = this.fileBasedExportedDataAccessor.GetExportDirectory(tenant);
            if (!Directory.Exists(directory)) return;

            logger.LogInformation("Deleting export archives for tenant: {tenant}, daysToKeep: {daysToKeep}, countToKeep: {countToKeep}",
                tenant, daysToKeep, countToKeep);

            if (!countToKeep.HasValue && !daysToKeep.HasValue)
            {
                Directory.Delete(directory, true);
                return;
            }

            var files = Directory.GetFiles(directory, "*.zip", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .Where(f => !f.FullName.Contains("temp"))
                .OrderBy(f => f.LastWriteTimeUtc)
                .ToList();

            int? countToDelete = countToKeep.HasValue ? files.Count - countToKeep.Value : null;

            foreach (var file in files)
            {
                await DeleteFile(file.FullName, file.LastWriteTimeUtc, tenant, countToDelete, daysToKeep, path =>
                {
                    File.Delete(path);
                    return Task.CompletedTask;
                });
                if (countToDelete.HasValue) countToDelete--;
            }
        }

        private async Task DeleteFile(string path, DateTime lastModified, TenantInfo tenant, int? countToDelete, 
            int? daysToKeep, Func<string, Task> deleteAction)
        {
            try
            {
                if (daysToKeep.HasValue && lastModified < DateTime.UtcNow.AddDays(-daysToKeep.Value))
                {
                    await deleteAction(path);
                    logger.LogInformation("Export archive {path} deleted for tenant:{tenant}", path, tenant);
                }
                else if (countToDelete is > 0)
                {
                    await deleteAction(path);
                    logger.LogInformation("Export archive {path} deleted for tenant:{tenant}", path, tenant);
                }
            }
            catch (Exception e)
            {
                logger.LogInformation("Unable to delete export archive {path} for tenant: {tenant}. Reason: {reason}", 
                    path, tenant, e.Message);
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

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
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
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportArchiveHandleService(IFileBasedExportedDataAccessor fileBasedExportedDataAccessor,
            IExternalArtifactsStorage externalArtifactsStorage,
            IDataExportFileAccessor exportFileAccessor,
            ILogger<JobsStatusReporting> logger,
            IExportFileNameService fileNameService,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.fileBasedExportedDataAccessor = fileBasedExportedDataAccessor;
            this.externalArtifactsStorage = externalArtifactsStorage;
            this.exportFileAccessor = exportFileAccessor;
            this.logger = logger;
            this.fileNameService = fileNameService;
            this.fileSystemAccessor = fileSystemAccessor;
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

            logger.LogInformation(
                "Deleting export archives for tenant: {tenant}, total files: {count}, daysToKeep: {daysToKeep}, countToKeep: {countToKeep}",
                tenant, items?.Count, daysToKeep, countToKeep);

            if (items == null) return;
            
            int? countToDelete = countToKeep.HasValue ? items.Count - countToKeep.Value : null;

            foreach (var file in items.OrderBy(x => x.LastModified))
            {
                await DeleteFile(file.Path, file.LastModified.ToUniversalTime(), tenant, countToDelete, daysToKeep, async path =>
                {
                    await this.externalArtifactsStorage.RemoveAsync(path);
                });
                if (countToDelete.HasValue) countToDelete--;
            }
        }

        private async Task DeleteFromLocalStorage(TenantInfo tenant, int? countToKeep, int? daysToKeep)
        {
            var directory = this.fileBasedExportedDataAccessor.GetExportDirectory(tenant);
            if (!this.fileSystemAccessor.IsDirectoryExists(directory)) return;

            logger.LogInformation("Deleting export archives for tenant: {tenant}, daysToKeep: {daysToKeep}, countToKeep: {countToKeep}",
                tenant, daysToKeep, countToKeep);

            if (!countToKeep.HasValue && !daysToKeep.HasValue)
            {
                this.fileSystemAccessor.DeleteDirectory(directory);
                return;
            }

            var tempFolderMarker = "temp" + fileSystemAccessor.DirectorySeparatorChar();
            
            var files = this.fileSystemAccessor.GetFilesInDirectory(directory, "*.zip", true)
                .Select(f => new {FullName = f, LastWriteTimeUtc = fileSystemAccessor.GetModificationTime(f).ToUniversalTime()})
                .Where(f => !f.FullName.Contains(tempFolderMarker))
                .OrderBy(f => f.LastWriteTimeUtc)
                .ToList();

            int? countToDelete = countToKeep.HasValue ? files.Count - countToKeep.Value : null;

            foreach (var file in files)
            {
                await DeleteFile(file.FullName, file.LastWriteTimeUtc, tenant, countToDelete, daysToKeep, path =>
                {
                    this.fileSystemAccessor.DeleteFile(path);
                    return Task.CompletedTask;
                });
                if (countToDelete.HasValue) countToDelete--;
            }
        }

        private async Task DeleteFile(string path, DateTime lastModifiedUtc, TenantInfo tenant, int? countToDelete, 
            int? daysToKeep, Func<string, Task> deleteAction)
        {
            try
            {
                if (
                    (!countToDelete.HasValue && !daysToKeep.HasValue)
                    || (daysToKeep.HasValue && lastModifiedUtc < DateTime.UtcNow.AddDays(-daysToKeep.Value)) 
                    || countToDelete is > 0)
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
                    settings.Tenant, this.fileSystemAccessor.GetFileName(internalFilePath));

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
                if (this.fileSystemAccessor.IsFileExists(filePath))
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

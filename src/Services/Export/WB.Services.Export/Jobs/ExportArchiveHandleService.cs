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

        public async Task ClearExportArchives(TenantInfo tenant, int? countToKeep, int? daysToKeep)
        {
            if (this.externalArtifactsStorage.IsEnabled())
            {
                var externalStoragePath = this.exportFileAccessor.GetExternalStoragePath(tenant, string.Empty);
                var items = await this.externalArtifactsStorage.ListAsync(externalStoragePath);
                if(items == null) return;

                logger.LogInformation("Deleting export archives for tenant: {tenant} - there are total {count} files", tenant, items.Count);

                int? countToDelete = countToKeep.HasValue ? items.Count - countToKeep.Value : null;
                foreach (var file in items.OrderByDescending(x => x.LastModified))
                {
                    if (!countToKeep.HasValue && !daysToKeep.HasValue)
                    {
                        await this.externalArtifactsStorage.RemoveAsync(file.Path);
                        continue;
                    }

                    if (daysToKeep.HasValue && file.LastModified < DateTime.UtcNow.AddDays(-daysToKeep.Value))
                    { 
                        await this.externalArtifactsStorage.RemoveAsync(file.Path);
                        if(countToDelete.HasValue)
                            countToDelete--;
                    }
                    else if (countToDelete is > 0)
                    {
                        await this.externalArtifactsStorage.RemoveAsync(file.Path);
                        countToDelete--;
                    }
                }

                return;
            }

            var directory = this.fileBasedExportedDataAccessor.GetExportDirectory(tenant);
            if (Directory.Exists(directory))
            {
                logger.LogInformation("Deleting export archives for tenant: {tenant}", tenant);
                
                if(!countToKeep.HasValue && !daysToKeep.HasValue)
                    Directory.Delete(directory, true);
                else
                {
                    //get all files infos in directory recursively
                    var files = Directory.GetFiles(directory, "*.zip", SearchOption.AllDirectories)
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.LastWriteTimeUtc)
                        .ToList();
                    
                    int? countToDelete = countToKeep.HasValue ? files.Count - countToKeep.Value : null;

                    foreach (var file in files)
                    {
                        if (daysToKeep.HasValue && file.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-daysToKeep.Value))
                        { 
                            File.Delete(file.FullName);
                            if(countToDelete.HasValue)
                                countToDelete--;
                        }
                        else if (countToDelete is > 0)
                        {
                            File.Delete(file.FullName);
                            countToDelete--;
                        }
                    }
                }
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

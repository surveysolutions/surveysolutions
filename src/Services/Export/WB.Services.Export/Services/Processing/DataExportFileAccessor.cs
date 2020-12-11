using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Storage;
using WB.Services.Infrastructure.FileSystem;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    class DataExportFileAccessor : IDataExportFileAccessor
    {
        private readonly IArchiveUtils archiveUtils;
        private readonly IExternalArtifactsStorage externalArtifactsStorage;

        public DataExportFileAccessor(IArchiveUtils archiveUtils, IExternalArtifactsStorage externalArtifactsStorage)
        {
            this.archiveUtils = archiveUtils;
            this.externalArtifactsStorage = externalArtifactsStorage;
        }

        public string GetExternalStoragePath(TenantInfo tenant, string name) => $"{tenant.Name}/{name}";

        public IZipArchive CreateExportArchive(Stream outputStream, string? password = null, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            return archiveUtils.CreateArchive(outputStream, password, compressionLevel);
        }

        public void RecreateExportArchive(string exportTempDirectoryPath, string archiveName, string archivePassword, ExportProgress exportProgress)
        {
            this.archiveUtils.ZipDirectory(exportTempDirectoryPath, archiveName, archivePassword, new Progress<int>((v) => exportProgress.Report(v)));
        }

        public void RecreateExportArchive(string exportTempDirectoryPath, IEnumerable<string> filesToArchive, string archiveFilePath, string archivePassword)
        {
            this.archiveUtils.ZipFiles(exportTempDirectoryPath, filesToArchive, archiveFilePath, archivePassword);
        }

        public async Task<bool> PublishArchiveToArtifactsStorageAsync(TenantInfo tenant, string archiveFile, ExportProgress exportProgress, CancellationToken cancellationToken)
        {
            if (externalArtifactsStorage.IsEnabled())
            {
                using var file = File.OpenRead(archiveFile);
                var name = Path.GetFileName(archiveFile);
                    
                await this.externalArtifactsStorage.StoreAsync(
                    GetExternalStoragePath(tenant, name), file, "application/zip",
                    exportProgress, cancellationToken);

                return true;
            }

            return false;
        }
    }
}

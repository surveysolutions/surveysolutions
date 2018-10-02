using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Export.Tenant;
using WB.Services.Infrastructure.FileSystem;

namespace WB.Services.Export.Services.Processing
{
    class DataExportFileAccessor : IDataExportFileAccessor
    {
        private readonly IArchiveUtils archiveUtils;
        private readonly IExternalFileStorage externalFileStorage;

        public DataExportFileAccessor(IArchiveUtils archiveUtils, IExternalFileStorage externalFileStorage)
        {
            this.archiveUtils = archiveUtils;
            this.externalFileStorage = externalFileStorage;
        }

        public string GetExternalStoragePath(TenantInfo tenant, string name) => $"{tenant.Id}/{name}";
        public IZipArchive CreateExportArchive(Stream outputStream, string password = null, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            return archiveUtils.CreateArchive(outputStream, password, compressionLevel);
        }

        public void RecreateExportArchive(string exportTempDirectoryPath, string archiveName, string archivePassword, IProgress<int> exportProgress)
        {
            //   throw new NotImplementedException();
            this.archiveUtils.ZipDirectory(exportTempDirectoryPath, archiveName, archivePassword, exportProgress);
        }

        public void RecreateExportArchive(string exportTempDirectoryPath, IEnumerable<string> filesToArchive, string archiveFilePath, string archivePassword)
        {
            this.archiveUtils.ZipFiles(exportTempDirectoryPath, filesToArchive, archiveFilePath, archivePassword);
        }

        public async Task PublishArchiveToExternalStorageAsync(TenantInfo tenant, string archiveFile, IProgress<int> exportProgress)
        {
            if (externalFileStorage.IsEnabled())
            {
                using (var file = File.OpenRead(archiveFile))
                {
                    var name = Path.GetFileName(archiveFile);
                    await this.externalFileStorage.StoreAsync(GetExternalStoragePath(tenant, name), file, "application/zip", exportProgress);
                }

                File.Delete(archiveFile);
            }
        }
    }
}

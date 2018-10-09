using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using WB.Services.Infrastructure.FileSystem;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportFileAccessor
    {
        string GetExternalStoragePath(TenantInfo tenant, string name);
        IZipArchive CreateExportArchive(Stream outputStream, string archivePassword, CompressionLevel compressionLevel = CompressionLevel.Fastest);
        void RecreateExportArchive(string exportTempDirectoryPath, string archiveName, string archivePassword, IProgress<int> exportProgress);
        Task PublishArchiveToExternalStorageAsync(TenantInfo tenant, string archiveFile, IProgress<int> exportProgress);
        void RecreateExportArchive(string exportTempDirectoryPath, IEnumerable<string> filesToArchive, string archiveFilePath, string password);
    }
}

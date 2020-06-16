using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Infrastructure.FileSystem;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportFileAccessor
    {
        string GetExternalStoragePath(TenantInfo tenant, string name);
        IZipArchive CreateExportArchive(Stream outputStream, string? archivePassword, CompressionLevel compressionLevel = CompressionLevel.Fastest);
        void RecreateExportArchive(string exportTempDirectoryPath, string archiveName, string? archivePassword, ExportProgress exportProgress);
        Task<bool> PublishArchiveToArtifactsStorageAsync(TenantInfo tenant, string archiveFile, ExportProgress exportProgress, CancellationToken cancellationToken);
        void RecreateExportArchive(string exportTempDirectoryPath, IEnumerable<string> filesToArchive, string archiveFilePath, string? password);
    }
}

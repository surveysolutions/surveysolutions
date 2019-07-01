using System;
using System.IO;
using Ionic.Zlib;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportFileAccessor
    {
        void PublishArchiveToExternalStorage(string archiveFile, IProgress<int> exportProgress);
        IZipArchive CreateExportArchive(Stream outputStream, CompressionLevel compressionLevel = CompressionLevel.BestSpeed);
        void RecreateExportArchive(string folderForDataExport, string archiveFilePath, IProgress<int> progress = null);
        string GetExternalStoragePath(string name);
    }
}

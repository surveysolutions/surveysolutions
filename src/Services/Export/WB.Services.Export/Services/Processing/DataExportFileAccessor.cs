using System;
using System.IO;
using System.IO.Compression;
using WB.Services.Export.Services.Processing.Good;
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

        public string GetExternalStoragePath(string name) => $"export/" + name;
        public IZipArchive CreateExportArchive(Stream outputStream, string password = null, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            return archiveUtils.CreateArchive(outputStream, password, compressionLevel);
        }

        public void RecreateExportArchive(string exportTempDirectoryPath, string archiveName, string archivePassword, IProgress<int> exportProgress)
        {
            throw new NotImplementedException();
            //this.archiveUtils.ZipDirectory(exportTempDirectoryPath, archiveName, archivePassword, exportProgress);
        }

        public void PubishArchiveToExternalStorage(string archiveFile, IProgress<int> exportProgress)
        {
            if (externalFileStorage.IsEnabled())
            {
                using (var file = File.OpenRead(archiveFile))
                {
                    var name = Path.GetFileName(archiveFile);
                    this.externalFileStorage.Store(GetExternalStoragePath(name), file, "application/zip", exportProgress);
                }
            }
        }

    }
}

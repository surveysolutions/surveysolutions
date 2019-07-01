using System;
using System.IO;
using Ionic.Zlib;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public class DataExportFileAccessor : IDataExportFileAccessor
    {
        private readonly IExportSettings exportSettings;
        private readonly IProtectedArchiveUtils archiveUtils;
        private readonly IExternalFileStorage externalFileStorage;
        private readonly ILogger logger;

        public DataExportFileAccessor(IExportSettings exportSettings, 
            IProtectedArchiveUtils archiveUtils,
            ILogger logger, 
            IExternalFileStorage externalFileStorage)
        {
            this.exportSettings = exportSettings;
            this.archiveUtils = archiveUtils;
            this.logger = logger;
            this.externalFileStorage = externalFileStorage;
        }

        public void RecreateExportArchive(string folderForDataExport, string archiveFilePath, IProgress<int> progress = null)
        {
            var password = this.GetPasswordFromSettings();
            this.archiveUtils.ZipDirectory(folderForDataExport, archiveFilePath, password, progress);
        }

        public IZipArchive CreateExportArchive(Stream outputStream, CompressionLevel compressionLevel = CompressionLevel.BestSpeed)
        {
            var password = this.GetPasswordFromSettings();
            return new IonicZipArchive(outputStream, password, compressionLevel);
        }

        public string GetExternalStoragePath(string name) => $"export/" + name;

        public void PublishArchiveToExternalStorage(string archiveFile, IProgress<int> exportProgress)
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

        private string GetPasswordFromSettings()
        {
            return this.exportSettings.EncryptionEnforced()
                    ? this.exportSettings.GetPassword()
                    : null;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Security;
using System;

namespace WB.Infrastructure.Native.Files.Implementation.FileSystem
{
    public class ZipArchiveUtilsWithEncryptionDecorator : IArchiveUtils
    {
        private readonly IArchiveUtils archiveUtils;
        private IExportSettings exportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ZipArchiveUtilsWithEncryptionDecorator(IArchiveUtils archiveUtils, IFileSystemAccessor fileSystemAccessor, IExportSettings exportSettings)
        {
            this.archiveUtils = archiveUtils;
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportSettings = exportSettings;
        }

        public string CompressString(string stringToCompress)
        {
            return this.archiveUtils.CompressString(stringToCompress);
        }

        public byte[] CompressStringToByteArray(string fileName, string fileContentAsString)
        {
            return this.archiveUtils.CompressStringToByteArray(fileName, fileContentAsString);
        }

        public string DecompressString(string stringToDecompress)
        {
            return this.archiveUtils.DecompressString(stringToDecompress);
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath)
        {
            return this.archiveUtils.GetArchivedFileNamesAndSize(filePath);
        }

        public bool IsZipFile(string filePath)
        {
            return this.archiveUtils.IsZipFile(filePath);
        }

        public bool IsZipStream(Stream zipStream)
        {
            return this.archiveUtils.IsZipStream(zipStream);
        }

        public void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            this.archiveUtils.Unzip(archivedFile, extractToFolder, ignoreRootDirectory);
        }

        public Task UnzipAsync(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            return this.archiveUtils.UnzipAsync(archivedFile, extractToFolder, ignoreRootDirectory);
        }

        public IEnumerable<UnzippedFile> UnzipStream(Stream zipStream)
        {
            return this.archiveUtils.UnzipStream(zipStream);
        }

        public void ZipDirectory(string directory, string archiveFile)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFile))
                throw new InvalidOperationException("zip file exists");

            using (var zipFile = new ZipFile()
            {
                ParallelDeflateThreshold = -1,
                AlternateEncoding = System.Text.Encoding.UTF8,
                AlternateEncodingUsage = ZipOption.Always
            })
            {
                if (this.exportSettings!= null && this.exportSettings.EncryptionEnforced())
                    zipFile.Password = this.exportSettings.GetPassword();

                zipFile.AddDirectory(directory, this.fileSystemAccessor.GetFileName(directory));
                zipFile.Save(archiveFile);
            }
        }

        public byte[] ZipDirectoryToByteArray(string sourceDirectory, string directoryFilter = null, string fileFilter = null)
        {
            return this.archiveUtils.ZipDirectoryToByteArray(sourceDirectory, directoryFilter, fileFilter);
        }

        public Task<byte[]> ZipDirectoryToByteArrayAsync(string sourceDirectory, string directoryFilter = null, string fileFilter = null)
        {
            return this.archiveUtils.ZipDirectoryToByteArrayAsync(sourceDirectory, directoryFilter, fileFilter);
        }

        public void ZipFiles(IEnumerable<string> files, string archiveFilePath)
        {
            using (var zip = new ZipFile(this.fileSystemAccessor.GetFileName(archiveFilePath)))
            {
                if(this.exportSettings != null && this.exportSettings.EncryptionEnforced())
                    zip.Password = this.exportSettings.GetPassword();

                zip.CompressionLevel = CompressionLevel.Default;
                zip.AddFiles(files, "");
                zip.Save(archiveFilePath);
            }
        }
    }
}
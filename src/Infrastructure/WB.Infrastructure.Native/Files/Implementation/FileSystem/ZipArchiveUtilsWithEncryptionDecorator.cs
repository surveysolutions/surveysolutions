using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.Infrastructure.FileSystem;
using System;
using ZipEntry = Ionic.Zip.ZipEntry;
using ZipFile = Ionic.Zip.ZipFile;

namespace WB.Infrastructure.Native.Files.Implementation.FileSystem
{
    public class ZipArchiveUtilsWithEncryptionDecorator : IArchiveUtils
    {
        private readonly IArchiveUtils archiveUtils;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ZipArchiveUtilsWithEncryptionDecorator(IArchiveUtils archiveUtils, IFileSystemAccessor fileSystemAccessor)
        {
            this.archiveUtils = archiveUtils;
            this.fileSystemAccessor = fileSystemAccessor;
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

        public Stream GetZipWithPassword(Stream inputZipStream, string password)
        {
            var outputZipStream = new MemoryStream();
            using (var zipFile = ZipFile.Read(inputZipStream))
            {
                zipFile.Password = password;
                foreach (ZipEntry zipEntry in zipFile)
                {
                    zipEntry.Password = password;
                }
                zipFile.Save(outputZipStream);
            }
            outputZipStream.Position = 0;
            return outputZipStream;
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

            using (var outputSteam = this.fileSystemAccessor.OpenOrCreateFile(archiveFile, false))
            {
                using (var zipFile = new ZipFile()
                {
                    ParallelDeflateThreshold = -1,
                    AlternateEncoding = System.Text.Encoding.UTF8,
                    AlternateEncodingUsage = ZipOption.Always
                })
                {
                    foreach (var filePath in this.fileSystemAccessor.GetFilesInDirectory(directory, true))
                    {
                        var filePathInZip = $"{this.fileSystemAccessor.GetFileName(directory)}\\{filePath.Substring(directory.Length).TrimStart('\\')}";
                        zipFile.AddEntry(filePathInZip, this.fileSystemAccessor.ReadFile(filePath));
                    }
                    zipFile.Save(outputSteam);
                }
            }
        }

        public void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath, string directoryFilter = null,
            string fileFilter = null)
        {
            this.archiveUtils.ZipDirectoryToFile(sourceDirectory, archiveFilePath, directoryFilter, fileFilter);
        }

        public Task ZipDirectoryToFileAsync(string sourceDirectory, string archiveFilePath, string directoryFilter = null,
            string fileFilter = null)
        {
            return this.archiveUtils.ZipDirectoryToFileAsync(sourceDirectory, archiveFilePath, directoryFilter, fileFilter);
        }

        public void ZipFiles(IEnumerable<string> files, string archiveFilePath)
        {
            using (var zip = new ZipFile(this.fileSystemAccessor.GetFileName(archiveFilePath)))
            {
                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.CompressionLevel = CompressionLevel.Default;
                zip.AddFiles(files, "");
                zip.Save(archiveFilePath);
            }
        }
    }
}
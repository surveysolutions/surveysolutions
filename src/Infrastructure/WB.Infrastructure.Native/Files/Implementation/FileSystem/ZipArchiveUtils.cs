using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Infrastructure.Native.Files.Implementation.FileSystem
{
    public class ZipArchiveUtils : IArchiveUtils
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ZipArchiveUtils(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
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
                zipFile.AddDirectory(directory, this.fileSystemAccessor.GetFileName(directory));
                zipFile.Save(archiveFile);
            }
        }

        public void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath, string directoryFilter = null,
            string fileFilter = null)
        {
            throw new NotImplementedException();
        }

        public Task ZipDirectoryToFileAsync(string sourceDirectory, string archiveFilePath, string directoryFilter = null,
            string fileFilter = null)
        {
            return Task.Run(() => this.ZipDirectoryToFile(sourceDirectory, archiveFilePath, directoryFilter, fileFilter));
        }

        public void ZipFiles(IEnumerable<string> files, string archiveFilePath)
        {
            using (var zip = new ZipFile(this.fileSystemAccessor.GetFileName(archiveFilePath)))
            {
                zip.CompressionLevel = CompressionLevel.Default;
                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.AddFiles(files, "");
                zip.Save(archiveFilePath);
            }
        }

        public void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            using (ZipFile decompress = ZipFile.Read(archivedFile))
            {
                decompress.ExtractAll(extractToFolder, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public Task UnzipAsync(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            return Task.Run(() => this.Unzip(archivedFile, extractToFolder, ignoreRootDirectory));
        }

        public IEnumerable<UnzippedFile> UnzipStream(Stream zipStream)
        {
            zipStream.Seek(0, SeekOrigin.Begin);
            
            foreach (var zipEntry in ZipFile.Read(zipStream).Entries.Where(x => !x.IsDirectory))
            {
                var unzippedFileStream = new MemoryStream();
                zipEntry.Extract(unzippedFileStream);
                yield return new UnzippedFile
                {
                    FileName = zipEntry.FileName,
                    FileBytes = unzippedFileStream.ToArray()
                };
            }
        }

        public bool IsZipFile(string filePath)
        {
            return ZipFile.IsZipFile(filePath);
        }

        public bool IsZipStream(Stream zipStream)
        {
            return ZipFile.IsZipFile(zipStream, false);
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath)
        {
            var result = new Dictionary<string, long>();
            using (var zips = ZipFile.Read(filePath))
            {
                foreach (var zip in zips)
                {
                    if(zip.IsDirectory)
                        continue;
                    result.Add(zip.FileName, zip.UncompressedSize);
                }
            }
            return result;
        }

        public byte[] CompressStringToByteArray(string fileName, string fileContentAsString)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddEntry(fileName, Encoding.Unicode.GetBytes(fileContentAsString));
                    zip.Save(memoryStream);
                }

                return memoryStream.ToArray();
            }
        }

        public string CompressString(string stringToCompress)
        {
            var bytes = Encoding.Unicode.GetBytes(stringToCompress);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        public string DecompressString(string stringToDecompress)
        {
            var bytes = Convert.FromBase64String(stringToDecompress);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.Unicode.GetString(mso.ToArray());
            }
        }

        public Stream GetZipWithPassword(Stream inputZipStream, string password)
        {
            throw new NotImplementedException();
        }
    }
}

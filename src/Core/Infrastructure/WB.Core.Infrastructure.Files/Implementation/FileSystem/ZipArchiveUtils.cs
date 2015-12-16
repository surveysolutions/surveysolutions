using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.Infrastructure.Files.Implementation.FileSystem
{
    internal class ZipArchiveUtils : IArchiveUtils
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        
        public ZipArchiveUtils(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void ZipDirectory(string directory, string archiveFile)
        {
            if (fileSystemAccessor.IsFileExists(archiveFile))
                throw new InvalidOperationException("zip file exists");

            using (var zipFile = new ZipFile()
                {
                    ParallelDeflateThreshold = -1,
                    AlternateEncoding = System.Text.Encoding.UTF8,
                    AlternateEncodingUsage = ZipOption.Always
                })
            {
                zipFile.AddDirectory(directory, fileSystemAccessor.GetFileName(directory));
                zipFile.Save(archiveFile);
            }
        }

        public byte[] ZipDirectoryToByteArray(string sourceDirectory, string directoryFilter = null, string fileFilter = null)
        {
            throw new NotImplementedException();
        }

        public void ZipFiles(IEnumerable<string> files, string archiveFilePath)
        {
            using (var zip = new ZipFile(this.fileSystemAccessor.GetFileName(archiveFilePath)))
            {
                zip.CompressionLevel = CompressionLevel.BestCompression;

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

        public IEnumerable<UnzippedFile> UnzipStream(Stream zipStream)
        {
            zipStream.Seek(0, SeekOrigin.Begin);
            return ZipFile.Read(zipStream)
                .Entries.Where(x => !x.IsDirectory)
                .Select(x => new UnzippedFile
                {
                    FileName = x.FileName,
                    FileStream = x.InputStream
                });
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
    }
}

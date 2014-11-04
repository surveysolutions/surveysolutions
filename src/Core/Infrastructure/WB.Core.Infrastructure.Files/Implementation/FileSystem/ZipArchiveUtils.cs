using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void ZipFiles(IEnumerable<string> files, IEnumerable<string> directories, string archiveFilePath)
        {
            using (var zip = new ZipFile(this.fileSystemAccessor.GetFileName(archiveFilePath)))
            {
                zip.CompressionLevel = CompressionLevel.BestCompression;

                zip.AddFiles(files, "");

                foreach (var directory in directories)
                {
                    zip.AddDirectory(directory, "");
                }

                zip.Save(archiveFilePath);
            }
        }

        public void Unzip(string archivedFile, string extractToFolder)
        {
            using (ZipFile decompress = ZipFile.Read(archivedFile))
            {
                decompress.ExtractAll(extractToFolder, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public bool IsZipFile(string filePath)
        {
            return ZipFile.IsZipFile(filePath);
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
    }
}
